namespace AmarTools.Modules.CertificateGenerator.Contracts;

public sealed record CertificateFieldMappingInputDto(
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
