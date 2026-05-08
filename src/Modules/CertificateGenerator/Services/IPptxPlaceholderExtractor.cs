namespace AmarTools.Modules.CertificateGenerator.Services;

internal interface IPptxPlaceholderExtractor
{
    /// <summary>
    /// Reads the first slide of a PPTX stream and returns all distinct <<key>> placeholder
    /// keys found in text runs, in alphabetical order.
    /// The stream must be positioned at 0 before calling.
    /// </summary>
    IReadOnlyList<string> Extract(Stream pptxStream);
}
