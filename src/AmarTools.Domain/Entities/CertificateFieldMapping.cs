using AmarTools.BuildingBlocks.Domain;

namespace AmarTools.Domain.Entities;

/// <summary>
/// Defines how a single dataset column should be projected onto the certificate template.
/// </summary>
public sealed class CertificateFieldMapping : AuditableEntity
{
    public Guid CertificateTemplateConfigId { get; private set; }

    public CertificateTemplateConfig CertificateTemplateConfig { get; private set; } = null!;

    public string FieldKey { get; private set; } = string.Empty;

    public string SourceColumn { get; private set; } = string.Empty;

    public string FieldType { get; private set; } = "text";

    public double PositionX { get; private set; }

    public double PositionY { get; private set; }

    public double? Width { get; private set; }

    public double? Height { get; private set; }

    public double? FontSize { get; private set; }

    public string? FontColor { get; private set; }

    private CertificateFieldMapping() { }

    public static CertificateFieldMapping Create(
        Guid certificateTemplateConfigId,
        string fieldKey,
        string sourceColumn,
        string fieldType,
        double positionX,
        double positionY,
        double? width,
        double? height,
        double? fontSize,
        string? fontColor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceColumn);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldType);

        return new CertificateFieldMapping
        {
            CertificateTemplateConfigId = certificateTemplateConfigId,
            FieldKey = fieldKey.Trim(),
            SourceColumn = sourceColumn.Trim(),
            FieldType = fieldType.Trim().ToLowerInvariant(),
            PositionX = positionX,
            PositionY = positionY,
            Width = width,
            Height = height,
            FontSize = fontSize,
            FontColor = fontColor?.Trim()
        };
    }
}
