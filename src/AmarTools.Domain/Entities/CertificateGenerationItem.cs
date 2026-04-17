using AmarTools.BuildingBlocks.Domain;

namespace AmarTools.Domain.Entities;

/// <summary>
/// Stores one recipient row snapshot inside a certificate generation batch.
/// </summary>
public sealed class CertificateGenerationItem : AuditableEntity
{
    public Guid CertificateGenerationBatchId { get; private set; }

    public CertificateGenerationBatch CertificateGenerationBatch { get; private set; } = null!;

    public int SequenceNumber { get; private set; }

    public string RecipientName { get; private set; } = string.Empty;

    public string? RecipientEmail { get; private set; }

    public string PayloadJson { get; private set; } = "{}";

    public string Status { get; private set; } = "pending";

    public string? GeneratedFilePath { get; private set; }

    private CertificateGenerationItem() { }

    public static CertificateGenerationItem Create(
        Guid certificateGenerationBatchId,
        int sequenceNumber,
        string recipientName,
        string? recipientEmail,
        string payloadJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);

        return new CertificateGenerationItem
        {
            CertificateGenerationBatchId = certificateGenerationBatchId,
            SequenceNumber = sequenceNumber,
            RecipientName = recipientName.Trim(),
            RecipientEmail = string.IsNullOrWhiteSpace(recipientEmail) ? null : recipientEmail.Trim(),
            PayloadJson = payloadJson,
            Status = "pending"
        };
    }
}
