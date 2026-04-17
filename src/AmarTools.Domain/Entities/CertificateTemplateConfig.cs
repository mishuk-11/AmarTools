using AmarTools.BuildingBlocks.Domain;

namespace AmarTools.Domain.Entities;

/// <summary>
/// Stores the admin-configured setup for the Certificate Generator tool
/// within a specific event tool activation.
/// </summary>
public sealed class CertificateTemplateConfig : AuditableEntity
{
    public Guid EventToolId { get; private set; }

    public EventTool EventTool { get; private set; } = null!;

    public string TemplateName { get; private set; } = string.Empty;

    public string? BaseTemplatePath { get; private set; }

    public string? BaseTemplateFileName { get; private set; }

    public string? BaseTemplateFileType { get; private set; }

    public string? RecipientDatasetPath { get; private set; }

    public string? RecipientDatasetFileName { get; private set; }

    public string? EmailSubject { get; private set; }

    public string? EmailBody { get; private set; }

    public ICollection<CertificateFieldMapping> FieldMappings { get; private set; } =
        new List<CertificateFieldMapping>();

    private CertificateTemplateConfig() { }

    public static CertificateTemplateConfig Create(Guid eventToolId, string templateName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);

        return new CertificateTemplateConfig
        {
            EventToolId = eventToolId,
            TemplateName = templateName.Trim()
        };
    }

    public void UpdateMetadata(string templateName, string? emailSubject, string? emailBody)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);

        TemplateName = templateName.Trim();
        EmailSubject = emailSubject?.Trim();
        EmailBody = emailBody?.Trim();
    }

    public void SetBaseTemplate(string storagePath, string fileName, string fileType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileType);

        BaseTemplatePath = storagePath;
        BaseTemplateFileName = fileName.Trim();
        BaseTemplateFileType = fileType.Trim().ToLowerInvariant();
    }

    public void SetRecipientDataset(string storagePath, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        RecipientDatasetPath = storagePath;
        RecipientDatasetFileName = fileName.Trim();
    }
}
