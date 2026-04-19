namespace AmarTools.Modules.PhotoFrame.Contracts;

/// <summary>Admin view of the photo frame tool configuration for an event.</summary>
public sealed record PhotoFrameSetupDto(
    Guid      Id,
    Guid      EventToolId,
    string    EventName,
    string?   SponsorName,
    string?   VenueName,
    DateTime? EventDateTime,
    string?   FrameImageUrl,
    string?   LogoImageUrl,
    string?   SponsorLogoUrl,
    string    SharingSlug,
    string    SharingUrl,
    bool      IsPublished,
    LandingPageDto? LandingPage
);
