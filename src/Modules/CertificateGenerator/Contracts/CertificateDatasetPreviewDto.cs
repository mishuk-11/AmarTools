namespace AmarTools.Modules.CertificateGenerator.Contracts;

public sealed record CertificateDatasetPreviewDto(
    string FileName,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyDictionary<string, string>> SampleRows,
    int PreviewRowCount
);
