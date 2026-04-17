namespace AmarTools.Modules.CertificateGenerator.Services;

public sealed record CertificateParsedDatasetDto(
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyDictionary<string, string>> Rows
);
