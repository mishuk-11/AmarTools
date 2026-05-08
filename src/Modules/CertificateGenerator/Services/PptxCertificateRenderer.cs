using AmarTools.Domain.Entities;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;

namespace AmarTools.Modules.CertificateGenerator.Services;

internal sealed class PptxCertificateRenderer : IPptxCertificateRenderer
{
    public async Task<MemoryStream> RenderAsync(
        Stream templateStream,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        IReadOnlyList<CertificateFieldMapping> mappings,
        CancellationToken ct)
    {
        var outputStream = new MemoryStream();
        templateStream.Position = 0;
        await templateStream.CopyToAsync(outputStream, ct);
        outputStream.Position = 0;

        using (var doc = PresentationDocument.Open(outputStream, isEditable: true))
        {
            var presentationPart = doc.PresentationPart!;
            var slideIdList = presentationPart.Presentation!.SlideIdList!;
            var firstSlideId = slideIdList.Elements<SlideId>().First();
            var templateSlidePart = (SlidePart)presentationPart.GetPartById(firstSlideId.RelationshipId!);

            var maxSlideId = slideIdList.Elements<SlideId>().Max(s => s.Id!.Value);

            // Clone the template for rows 2..N before modifying row 1
            var slideParts = new List<SlidePart> { templateSlidePart };
            for (var i = 1; i < rows.Count; i++)
            {
                maxSlideId++;
                slideParts.Add(CloneSlide(presentationPart, templateSlidePart, maxSlideId));
            }

            // Fill placeholders per slide
            for (var i = 0; i < rows.Count; i++)
            {
                var slide = slideParts[i].Slide;
                if (slide is null) continue;
                ReplacePlaceholders(slide, rows[i], mappings);
                slide.Save();
            }

            presentationPart.Presentation.Save();
        }

        outputStream.Position = 0;
        return outputStream;
    }

    public async Task<MemoryStream> RenderSingleAsync(
        Stream templateStream,
        IReadOnlyDictionary<string, string> row,
        IReadOnlyList<CertificateFieldMapping> mappings,
        CancellationToken ct)
    {
        var outputStream = new MemoryStream();
        templateStream.Position = 0;
        await templateStream.CopyToAsync(outputStream, ct);
        outputStream.Position = 0;

        using (var doc = PresentationDocument.Open(outputStream, isEditable: true))
        {
            var presentationPart = doc.PresentationPart!;
            var slideIdList = presentationPart.Presentation!.SlideIdList!;
            var allSlideIds = slideIdList.Elements<SlideId>().ToList();
            var firstSlideId = allSlideIds.First();

            // Remove all slides except the first
            foreach (var extra in allSlideIds.Skip(1))
            {
                var part = (SlidePart)presentationPart.GetPartById(extra.RelationshipId!);
                presentationPart.DeletePart(part);
                extra.Remove();
            }

            var slidePart = (SlidePart)presentationPart.GetPartById(firstSlideId.RelationshipId!);
            var slide = slidePart.Slide;
            if (slide is not null)
            {
                ReplacePlaceholders(slide, row, mappings);
                slide.Save();
            }

            presentationPart.Presentation.Save();
        }

        outputStream.Position = 0;
        return outputStream;
    }

    private static SlidePart CloneSlide(
        PresentationPart presentationPart,
        SlidePart sourcePart,
        uint newSlideId)
    {
        var newSlidePart = presentationPart.AddNewPart<SlidePart>();

        using (var stream = sourcePart.GetStream())
            newSlidePart.FeedData(stream);

        // Copy all related parts (layout, images, etc.) with original relationship IDs
        // so that embedded resources remain resolved in the cloned slide XML.
        foreach (var partRef in sourcePart.Parts)
            if (partRef.OpenXmlPart is not null)
                newSlidePart.AddPart(partRef.OpenXmlPart, partRef.RelationshipId);

        presentationPart.Presentation!.SlideIdList!.Append(new SlideId
        {
            Id = newSlideId,
            RelationshipId = presentationPart.GetIdOfPart(newSlidePart)
        });

        return newSlidePart;
    }

    private static void ReplacePlaceholders(
        Slide slide,
        IReadOnlyDictionary<string, string> row,
        IReadOnlyList<CertificateFieldMapping> mappings)
    {
        foreach (var para in slide.Descendants<A.Paragraph>())
            ProcessParagraph(para, row, mappings);
    }

    private static void ProcessParagraph(
        A.Paragraph para,
        IReadOnlyDictionary<string, string> row,
        IReadOnlyList<CertificateFieldMapping> mappings)
    {
        var runs = para.Elements<A.Run>().ToList();
        if (runs.Count == 0) return;

        // Merge all run texts — PowerPoint can split a placeholder across runs
        var fullText = string.Concat(runs.Select(r => r.Text?.Text ?? string.Empty));

        var replacedText = fullText;
        foreach (var mapping in mappings)
        {
            var placeholder = $"<<{mapping.FieldKey}>>";
            if (!replacedText.Contains(placeholder, StringComparison.OrdinalIgnoreCase))
                continue;

            var value = row.TryGetValue(mapping.SourceColumn, out var v) ? v ?? string.Empty : string.Empty;
            replacedText = replacedText.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
        }

        if (replacedText == fullText) return;

        // Preserve the first run's character properties for consistent formatting
        var firstRunProps = runs[0].RunProperties?.CloneNode(deep: true) as A.RunProperties;

        foreach (var run in runs)
            run.Remove();

        var newRun = new A.Run();
        if (firstRunProps != null)
            newRun.Append(firstRunProps);
        var textNode = new A.Text(replacedText);
        textNode.SetAttribute(new OpenXmlAttribute("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve"));
        newRun.Append(textNode);

        // Insert before EndParagraphRunProperties if present, otherwise append
        var endParaRPr = para.Elements<A.EndParagraphRunProperties>().FirstOrDefault();
        if (endParaRPr != null)
            endParaRPr.InsertBeforeSelf(newRun);
        else
            para.Append(newRun);
    }
}
