namespace AmarTools.Modules.CertificateGenerator.Contracts;

public sealed record CertificateFieldMappingDto(
    Guid Id,
    string FieldKey,
    string SourceColumn,
    string FieldType,
    double PositionX,
    double PositionY,
    double? Width,
    double? Height,
    double? FontSize,
    string? FontColor
);
