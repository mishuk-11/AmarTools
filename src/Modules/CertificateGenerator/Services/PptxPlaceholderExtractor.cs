using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using System.Text.RegularExpressions;
using A = DocumentFormat.OpenXml.Drawing;

namespace AmarTools.Modules.CertificateGenerator.Services;

internal sealed partial class PptxPlaceholderExtractor : IPptxPlaceholderExtractor
{
    [GeneratedRegex(@"<<([^<>\r\n]+)>>", RegexOptions.IgnoreCase)]
    private static partial Regex PlaceholderPattern();

    public IReadOnlyList<string> Extract(Stream pptxStream)
    {
        // Copy to MemoryStream — OpenXml SDK closes the provided stream on dispose
        using var ms = new MemoryStream();
        pptxStream.Position = 0;
        pptxStream.CopyTo(ms);
        ms.Position = 0;

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var doc = PresentationDocument.Open(ms, isEditable: false);
        var presentationPart = doc.PresentationPart;
        if (presentationPart is null) return [];

        var firstSlideId = presentationPart.Presentation?.SlideIdList?
            .Elements<SlideId>().FirstOrDefault();
        if (firstSlideId?.RelationshipId is null) return [];

        var slidePart = (SlidePart)presentationPart.GetPartById(firstSlideId.RelationshipId!);
        var slide = slidePart.Slide;
        if (slide is null) return [];

        // Scan merged paragraph text — placeholders are often split across multiple runs
        foreach (var para in slide.Descendants<A.Paragraph>())
        {
            var fullText = string.Concat(
                para.Elements<A.Run>().Select(r => r.Text?.Text ?? string.Empty));

            foreach (Match m in PlaceholderPattern().Matches(fullText))
                keys.Add(m.Groups[1].Value.Trim());
        }

        return [.. keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase)];
    }
}
