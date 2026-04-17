namespace AmarTools.Modules.PhotoFrame.Contracts;

/// <summary>Guest-facing landing page design data.</summary>
public sealed record LandingPageDto(
    string  TemplateName,
    string  BackgroundColor,
    string? BackgroundImageUrl,
    string? HeadlineText,
    string  InstructionText,
    string  DownloadButtonText,

    // Event details shown on the landing page
    string    EventName,
    string?   SponsorName,
    string?   VenueName,
    DateTime? EventDateTime,
    string?   LogoUrl,
    string?   FrameImageUrl
);
