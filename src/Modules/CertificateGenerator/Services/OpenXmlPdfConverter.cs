using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;
using PShape   = DocumentFormat.OpenXml.Presentation.Shape;
using PPicture = DocumentFormat.OpenXml.Presentation.Picture;
using PGroup   = DocumentFormat.OpenXml.Presentation.GroupShape;
using PBg      = DocumentFormat.OpenXml.Presentation.Background;

namespace AmarTools.Modules.CertificateGenerator.Services;

/// <summary>
/// Pure-.NET PPTX → PDF using DocumentFormat.OpenXml + PdfSharpCore.
/// Renders in layer order: slide master → slide layout → slide.
/// Handles solid fills, gradient fills, blip (image) fills, and theme colour refs.
/// </summary>
internal sealed class OpenXmlPdfConverter : IPdfConverter
{
    private const double EmuToPoint      = 72.0 / 914400.0;
    private const double DefaultInsetEmu = 45720;

    public bool   IsAvailable     => true;
    public string OutputExtension => "pdf";

    // ── Entry point ──────────────────────────────────────────────────────────

    public async Task<MemoryStream> ConvertFromPptxAsync(Stream pptxStream, CancellationToken ct)
    {
        var ms = new MemoryStream();
        pptxStream.Position = 0;
        await pptxStream.CopyToAsync(ms, ct);
        ms.Position = 0;

        using var presDoc = PresentationDocument.Open(ms, false);
        var presPart  = presDoc.PresentationPart!;
        var slidePart = presPart.SlideParts.First();

        var sldSz = presPart.Presentation?.SlideSize
                    ?? throw new InvalidOperationException("PPTX has no SlideSize.");
        double pageW = sldSz.Cx!.Value * EmuToPoint;
        double pageH = sldSz.Cy!.Value * EmuToPoint;

        using var pdf  = new PdfDocument();
        var page = pdf.AddPage();
        page.Width  = XUnit.FromPoint(pageW);
        page.Height = XUnit.FromPoint(pageH);

        var layoutPart = slidePart.SlideLayoutPart;
        var masterPart = layoutPart?.SlideMasterPart;

        // XGraphics MUST be disposed before pdf.Save — disposal flushes the page content stream.
        using (var gfx = XGraphics.FromPdfPage(page))
        {
            gfx.DrawRectangle(XBrushes.White, 0, 0, pageW, pageH);

            // Layer 1 – slide master
            if (masterPart is not null)
            {
                PaintBackground(gfx, masterPart.SlideMaster?.CommonSlideData?.Background, masterPart, pageW, pageH);
                RenderShapeTree(gfx, masterPart.SlideMaster?.CommonSlideData?.ShapeTree, masterPart, pageW, pageH);
            }

            // Layer 2 – slide layout
            if (layoutPart is not null)
            {
                PaintBackground(gfx, layoutPart.SlideLayout?.CommonSlideData?.Background, layoutPart, pageW, pageH);
                RenderShapeTree(gfx, layoutPart.SlideLayout?.CommonSlideData?.ShapeTree, layoutPart, pageW, pageH);
            }

            // Layer 3 – slide content
            PaintBackground(gfx, slidePart.Slide?.CommonSlideData?.Background, slidePart, pageW, pageH);
            RenderShapeTree(gfx, slidePart.Slide?.CommonSlideData?.ShapeTree, slidePart, pageW, pageH);
        }

        var result = new MemoryStream();
        pdf.Save(result, false);
        result.Position = 0;
        return result;
    }

    // ── Shape-tree traversal ─────────────────────────────────────────────────

    private static void RenderShapeTree(
        XGraphics gfx, OpenXmlElement? tree,
        OpenXmlPart part, double pageW, double pageH)
    {
        if (tree is null) return;
        foreach (var child in tree.ChildElements)
            RenderElement(gfx, child, part, pageW, pageH);
    }

    private static void RenderElement(
        XGraphics gfx, OpenXmlElement el,
        OpenXmlPart part, double pageW, double pageH)
    {
        switch (el)
        {
            case PPicture pic:
                RenderPicture(gfx, pic, part, pageW, pageH);
                break;
            case PShape shape:
                RenderShape(gfx, shape, part, pageW, pageH);
                break;
            case PGroup grp:
                foreach (var c in grp.ChildElements)
                    RenderElement(gfx, c, part, pageW, pageH);
                break;
        }
    }

    // ── Background ───────────────────────────────────────────────────────────

    private static void PaintBackground(
        XGraphics gfx, PBg? bg,
        OpenXmlPart part, double pageW, double pageH)
    {
        if (bg is null) return;

        // Inline fill definition (<p:bgPr>)
        if (bg.BackgroundProperties is { } bgPr)
        {
            TryRenderFill(gfx, bgPr, part, 0, 0, pageW, pageH);
            return;
        }

        // Theme style reference (<p:bgRef> — contains a colour child, e.g. <a:schemeClr>)
        // Use LocalName search because the typed property varies across SDK versions.
        var bgRef = bg.ChildElements.FirstOrDefault(e => e.LocalName == "bgRef");
        if (bgRef is not null)
        {
            var c = ResolveColorFromElement(bgRef, part);
            if (c.HasValue)
                gfx.DrawRectangle(new XSolidBrush(c.Value), 0, 0, pageW, pageH);
        }
    }

    // ── Picture shapes ───────────────────────────────────────────────────────

    private static void RenderPicture(
        XGraphics gfx, PPicture pic,
        OpenXmlPart part, double pageW, double pageH)
    {
        var xfrm = pic.ShapeProperties?.Transform2D;
        double x, y, w, h;
        if (xfrm is not null)
        {
            x = (xfrm.Offset?.X?.Value ?? 0) * EmuToPoint;
            y = (xfrm.Offset?.Y?.Value ?? 0) * EmuToPoint;
            w = (xfrm.Extents?.Cx?.Value ?? 0) * EmuToPoint;
            h = (xfrm.Extents?.Cy?.Value ?? 0) * EmuToPoint;
        }
        else { x = 0; y = 0; w = pageW; h = pageH; }

        if (w <= 0 || h <= 0) { w = pageW; h = pageH; }

        var rId = pic.BlipFill?.Blip?.Embed?.Value;
        if (rId is null) return;
        DrawImagePart(gfx, part, rId, x, y, w, h);
    }

    // ── Regular shapes ───────────────────────────────────────────────────────

    private static void RenderShape(
        XGraphics gfx, PShape shape,
        OpenXmlPart part, double pageW, double pageH)
    {
        var xfrm = shape.ShapeProperties?.Transform2D;
        if (xfrm is null) return;

        double x = (xfrm.Offset?.X?.Value ?? 0) * EmuToPoint;
        double y = (xfrm.Offset?.Y?.Value ?? 0) * EmuToPoint;
        double w = (xfrm.Extents?.Cx?.Value ?? 0) * EmuToPoint;
        double h = (xfrm.Extents?.Cy?.Value ?? 0) * EmuToPoint;
        if (w <= 0 || h <= 0) return;

        if (shape.ShapeProperties is not null)
            TryRenderFill(gfx, shape.ShapeProperties, part, x, y, w, h);

        var txBody = shape.TextBody;
        if (txBody is null) return;

        var bodyPr = txBody.BodyProperties;
        double inL = (bodyPr?.LeftInset?.Value  ?? DefaultInsetEmu) * EmuToPoint;
        double inT = (bodyPr?.TopInset?.Value   ?? DefaultInsetEmu) * EmuToPoint;
        double inR = (bodyPr?.RightInset?.Value ?? DefaultInsetEmu) * EmuToPoint;

        double tx  = x + inL;
        double tw  = Math.Max(1, w - inL - inR);
        double ty  = y + inT;
        double ty2 = y + h;

        foreach (var para in txBody.Elements<Paragraph>())
        {
            if (ty >= ty2) break;
            RenderParagraph(gfx, para, part, tx, ty, tw, ty2, out double lh);
            ty += lh;
        }
    }

    // ── Paragraph / text rendering ───────────────────────────────────────────

    private static void RenderParagraph(
        XGraphics gfx, Paragraph para,
        OpenXmlPart part,
        double x, double y, double w, double maxY,
        out double lineHeight)
    {
        lineHeight = 0;
        if (y >= maxY) return;

        var runs = para.Elements<Run>().ToList();
        if (runs.Count == 0) return;

        var text = string.Concat(runs.Select(r => r.Text?.Text ?? ""));
        if (string.IsNullOrEmpty(text)) return;

        var rPr     = runs[0].RunProperties;
        double ptSz = (rPr?.FontSize?.Value ?? 1800) / 100.0;
        bool bold   = rPr?.Bold?.Value   ?? false;
        bool italic = rPr?.Italic?.Value ?? false;

        XFontStyle style = (bold, italic) switch
        {
            (true,  true)  => XFontStyle.BoldItalic,
            (true,  false) => XFontStyle.Bold,
            (false, true)  => XFontStyle.Italic,
            _              => XFontStyle.Regular,
        };

        string fontName = rPr?.GetFirstChild<LatinFont>()?.Typeface?.Value ?? "Arial";
        if (string.IsNullOrWhiteSpace(fontName) || fontName.StartsWith('+'))
            fontName = "Arial";

        XFont font;
        try   { font = new XFont(fontName, ptSz, style); }
        catch { font = new XFont("Arial",  ptSz, XFontStyle.Regular); }

        XColor color = ResolveRunColor(rPr, part);

        var alignVal = para.ParagraphProperties?.Alignment?.Value;
        var align    = alignVal == TextAlignmentTypeValues.Center ? XParagraphAlignment.Center
                     : alignVal == TextAlignmentTypeValues.Right  ? XParagraphAlignment.Right
                     : XParagraphAlignment.Left;

        new XTextFormatter(gfx) { Alignment = align }
            .DrawString(text, font, new XSolidBrush(color), new XRect(x, y, w, maxY - y));

        lineHeight = gfx.MeasureString(text, font).Height + 2;
    }

    // ── Fill rendering ────────────────────────────────────────────────────────

    private static void TryRenderFill(
        XGraphics gfx, OpenXmlElement container,
        OpenXmlPart part, double x, double y, double w, double h)
    {
        // Explicit no-fill
        if (container.GetFirstChild<NoFill>() is not null) return;

        // Solid fill
        if (container.GetFirstChild<SolidFill>() is { } solid)
        {
            var c = ResolveColorFromElement(solid, part);
            if (c.HasValue)
                gfx.DrawRectangle(new XSolidBrush(c.Value), x, y, w, h);
            return;
        }

        // Gradient fill
        if (container.GetFirstChild<GradientFill>() is { } grad)
        {
            RenderGradientFill(gfx, grad, part, x, y, w, h);
            return;
        }

        // Blip (image) fill — both a:blipFill and p:blipFill
        var rId = container.GetFirstChild<DocumentFormat.OpenXml.Drawing.BlipFill>()?.Blip?.Embed?.Value
               ?? container.GetFirstChild<DocumentFormat.OpenXml.Presentation.BlipFill>()?.Blip?.Embed?.Value;
        if (rId is not null)
            DrawImagePart(gfx, part, rId, x, y, w, h);
    }

    private static void RenderGradientFill(
        XGraphics gfx, GradientFill grad,
        OpenXmlPart part, double x, double y, double w, double h)
    {
        var stops = grad.GradientStopList?.Elements<GradientStop>()
            .OrderBy(gs => gs.Position?.Value ?? 0)
            .ToList();
        if (stops is null || stops.Count == 0) return;

        var colors = stops
            .Select(s => ResolveColorFromElement(s, part))
            .Where(c => c.HasValue)
            .Select(c => c!.Value)
            .ToList();

        if (colors.Count == 0) return;
        if (colors.Count == 1) { gfx.DrawRectangle(new XSolidBrush(colors[0]), x, y, w, h); return; }

        // Angle is in 60000ths of a degree, measured clockwise from the top (like a compass).
        // 0 = top→bottom, 90°(=5400000) = left→right
        double angleDeg = 0.0;
        var linEl = grad.GetFirstChild<LinearGradientFill>();
        if (linEl?.Angle?.HasValue == true)
            angleDeg = linEl.Angle.Value / 60000.0;

        var (pt1, pt2) = GradientEndPoints(angleDeg, x, y, w, h);
        try
        {
            var brush = new XLinearGradientBrush(pt1, pt2, colors[0], colors[^1]);
            gfx.DrawRectangle(brush, x, y, w, h);
        }
        catch
        {
            gfx.DrawRectangle(new XSolidBrush(colors[0]), x, y, w, h);
        }
    }

    private static (XPoint start, XPoint end) GradientEndPoints(
        double angleDeg, double x, double y, double w, double h)
    {
        // OOXML: 0° = top→bottom. Convert to standard math angle (0°=right, CCW).
        var rad  = (90.0 - angleDeg) * Math.PI / 180.0;
        var cx   = x + w / 2.0;
        var cy   = y + h / 2.0;
        var half = Math.Sqrt(w * w + h * h) / 2.0;
        var dx   = Math.Cos(rad) * half;
        var dy   = -Math.Sin(rad) * half;
        return (new XPoint(cx - dx, cy - dy), new XPoint(cx + dx, cy + dy));
    }

    // ── Image part drawing ────────────────────────────────────────────────────

    private static void DrawImagePart(
        XGraphics gfx, OpenXmlPart part, string rId,
        double x, double y, double w, double h)
    {
        try
        {
            var imgPart = part.GetPartById(rId) as ImagePart;
            if (imgPart is null) return;

            var ct = imgPart.ContentType ?? string.Empty;
            if (ct.Contains("svg", StringComparison.OrdinalIgnoreCase) ||
                ct.Contains("emf", StringComparison.OrdinalIgnoreCase) ||
                ct.Contains("wmf", StringComparison.OrdinalIgnoreCase))
                return;

            byte[] imgBytes;
            using (var raw = imgPart.GetStream())
            using (var buf = new MemoryStream())
            {
                raw.CopyTo(buf);
                imgBytes = buf.ToArray();
            }
            if (imgBytes.Length == 0) return;

            using var img = XImage.FromStream(() => new MemoryStream(imgBytes));
            gfx.DrawImage(img, x, y, w, h);
        }
        catch { /* skip unrenderable images */ }
    }

    // ── Colour resolution ─────────────────────────────────────────────────────

    private static XColor ResolveRunColor(RunProperties? rPr, OpenXmlPart part)
    {
        if (rPr?.GetFirstChild<SolidFill>() is { } sf)
            return ResolveColorFromElement(sf, part) ?? XColors.Black;
        return XColors.Black;
    }

    /// <summary>
    /// Resolves a colour value from a containing element (SolidFill, GradientStop,
    /// BackgroundStyleReference, etc.).  The element is expected to have one of
    /// <a:srgbClr>, <a:sysClr>, or <a:schemeClr> as a direct child.
    /// </summary>
    private static XColor? ResolveColorFromElement(OpenXmlElement? el, OpenXmlPart? part)
    {
        if (el is null) return null;

        // Direct hex colour (<a:srgbClr val="RRGGBB"/>)
        if (el.GetFirstChild<RgbColorModelHex>()?.Val?.Value is { Length: 6 } hex1
            && TryParseHex(hex1, out var c1)) return c1;

        // System colour fallback (<a:sysClr lastClr="RRGGBB"/>)
        if (el.GetFirstChild<SystemColor>()?.LastColor?.Value is { Length: 6 } hex2
            && TryParseHex(hex2, out var c2)) return c2;

        // Scheme colour — resolve through theme colour scheme
        if (el.GetFirstChild<SchemeColor>() is { } schClr && part is not null)
            return ResolveSchemeColor(schClr.Val?.Value.ToString(), part);

        return null;
    }

    private static XColor? ResolveSchemeColor(string? name, OpenXmlPart part)
    {
        if (name is null) return null;

        var cs = GetThemePart(part)?.Theme?.ThemeElements?.ColorScheme;
        if (cs is null) return null;

        // Map OOXML scheme-colour names to ColorScheme child properties
        var colorEl = name switch
        {
            "dk1" or "tx1" or "text1"                => (Color2Type?)cs.Dark1Color,
            "dk2" or "tx2" or "text2"                => cs.Dark2Color,
            "lt1" or "bg1" or "background1"          => cs.Light1Color,
            "lt2" or "bg2" or "background2"          => cs.Light2Color,
            "accent1"                                => cs.Accent1Color,
            "accent2"                                => cs.Accent2Color,
            "accent3"                                => cs.Accent3Color,
            "accent4"                                => cs.Accent4Color,
            "accent5"                                => cs.Accent5Color,
            "accent6"                                => cs.Accent6Color,
            "hlink"                                  => cs.Hyperlink,
            "folHlink"                               => cs.FollowedHyperlinkColor,
            _                                        => null,
        };

        if (colorEl is null) return null;

        // Prefer <a:srgbClr>
        if (colorEl.RgbColorModelHex?.Val?.Value is { Length: 6 } hex
            && TryParseHex(hex, out var c)) return c;

        // Fallback to sysClr lastClr
        if (colorEl.SystemColor?.LastColor?.Value is { Length: 6 } last
            && TryParseHex(last, out c)) return c;

        return null;
    }

    private static ThemePart? GetThemePart(OpenXmlPart part) => part switch
    {
        SlideMasterPart smp => smp.ThemePart,
        SlideLayoutPart slp => slp.SlideMasterPart?.ThemePart,
        SlidePart       sp  => sp.SlideLayoutPart?.SlideMasterPart?.ThemePart,
        _                   => null,
    };

    private static bool TryParseHex(string? hex, out XColor color)
    {
        if (hex is { Length: 6 })
        {
            try
            {
                color = XColor.FromArgb(
                    Convert.ToInt32(hex[..2], 16),
                    Convert.ToInt32(hex[2..4], 16),
                    Convert.ToInt32(hex[4..6], 16));
                return true;
            }
            catch { }
        }
        color = default;
        return false;
    }
}
