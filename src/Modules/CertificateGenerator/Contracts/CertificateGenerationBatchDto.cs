namespace AmarTools.Modules.CertificateGenerator.Contracts;

public sealed record CertificateGenerationBatchDto(
    Guid Id,
    Guid CertificateTemplateConfigId,
    string Status,
    string OutputFormat,
    int TotalRecipients,
    IReadOnlyList<CertificateGenerationItemDto> Items
);
