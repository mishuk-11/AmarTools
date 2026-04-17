namespace AmarTools.Modules.CertificateGenerator.Contracts;

public sealed record CertificateTemplateSetupDto(
    Guid Id,
    Guid EventToolId,
    string TemplateName,
    string? BaseTemplateUrl,
    string? BaseTemplateFileName,
    string? BaseTemplateFileType,
    string? RecipientDatasetFileName,
    string? EmailSubject,
    string? EmailBody,
    IReadOnlyList<CertificateFieldMappingDto> FieldMappings
);
