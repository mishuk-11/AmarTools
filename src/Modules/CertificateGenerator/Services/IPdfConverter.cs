namespace AmarTools.Modules.CertificateGenerator.Services;

internal interface IPdfConverter
{
    /// <summary>True when the underlying converter tool is installed and usable.</summary>
    bool IsAvailable { get; }

    /// <summary>"pdf" when conversion is supported, "pptx" for the pass-through fallback.</summary>
    string OutputExtension { get; }

    /// <summary>
    /// Converts a single-slide PPTX stream to the output format.
    /// Returns a new MemoryStream positioned at 0.
    /// </summary>
    Task<MemoryStream> ConvertFromPptxAsync(Stream pptxStream, CancellationToken ct);
}
