namespace AmarTools.Modules.CertificateGenerator.Contracts;

public sealed record CertificateGenerationItemDto(
    Guid Id,
    int SequenceNumber,
    string RecipientName,
    string? RecipientEmail,
    string Status
);
