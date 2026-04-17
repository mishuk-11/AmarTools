using AmarTools.BuildingBlocks.Domain;

namespace AmarTools.Domain.Entities;

/// <summary>
/// Tracks a single batch generation request for a certificate template setup.
/// Rendering and email delivery can advance this batch over multiple processing steps.
/// </summary>
public sealed class CertificateGenerationBatch : AuditableEntity
{
    public Guid CertificateTemplateConfigId { get; private set; }

    public CertificateTemplateConfig CertificateTemplateConfig { get; private set; } = null!;

    public string Status { get; private set; } = "pending";

    public string OutputFormat { get; private set; } = "pdf";

    public int TotalRecipients { get; private set; }

    public int CompletedRecipients { get; private set; }

    public int FailedRecipients { get; private set; }

    public ICollection<CertificateGenerationItem> Items { get; private set; } =
        new List<CertificateGenerationItem>();

    private CertificateGenerationBatch() { }

    public static CertificateGenerationBatch Create(
        Guid certificateTemplateConfigId,
        string outputFormat,
        int totalRecipients)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFormat);

        return new CertificateGenerationBatch
        {
            CertificateTemplateConfigId = certificateTemplateConfigId,
            OutputFormat = outputFormat.Trim().ToLowerInvariant(),
            TotalRecipients = totalRecipients,
            Status = "pending"
        };
    }
}
