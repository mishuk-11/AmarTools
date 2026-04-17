namespace AmarTools.Domain.Enums;

/// <summary>
/// Identifies each purchasable tool in the AmarTools platform.
/// Stored as an integer in the database for efficient indexing.
/// </summary>
public enum ToolType
{
    EventPhotoframeGenerator = 1,
    CertificateGenerator     = 2,
    ChequePrinting           = 3,
    Voting                   = 4
}
