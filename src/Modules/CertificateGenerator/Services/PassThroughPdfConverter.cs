namespace AmarTools.Modules.CertificateGenerator.Services;

/// <summary>
/// Fallback converter used when LibreOffice is not installed.
/// Returns the PPTX stream unchanged so the pipeline still produces a valid ZIP.
/// </summary>
internal sealed class PassThroughPdfConverter : IPdfConverter
{
    public bool IsAvailable => true;
    public string OutputExtension => "pptx";

    public async Task<MemoryStream> ConvertFromPptxAsync(Stream pptxStream, CancellationToken ct)
    {
        var ms = new MemoryStream();
        pptxStream.Position = 0;
        await pptxStream.CopyToAsync(ms, ct);
        ms.Position = 0;
        return ms;
    }
}
