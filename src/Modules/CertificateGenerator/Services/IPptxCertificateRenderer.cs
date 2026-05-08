using AmarTools.Domain.Entities;

namespace AmarTools.Modules.CertificateGenerator.Services;

internal interface IPptxCertificateRenderer
{
    /// <summary>
    /// Clones slide 1 for each row, replaces &lt;&lt;FieldKey&gt;&gt; placeholders, and returns
    /// a single multi-slide PPTX containing all recipients.
    /// </summary>
    Task<MemoryStream> RenderAsync(
        Stream templateStream,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        IReadOnlyList<CertificateFieldMapping> mappings,
        CancellationToken ct);

    /// <summary>
    /// Renders a single-slide PPTX for one recipient row. Used when generating
    /// individual certificate files for ZIP packaging.
    /// The returned stream is positioned at 0.
    /// </summary>
    Task<MemoryStream> RenderSingleAsync(
        Stream templateStream,
        IReadOnlyDictionary<string, string> row,
        IReadOnlyList<CertificateFieldMapping> mappings,
        CancellationToken ct);
}
