namespace AmarTools.Modules.Auth.Contracts;

/// <summary>Lightweight user profile embedded in the auth token response.</summary>
public sealed record UserProfileDto(
    Guid   Id,
    string FullName,
    string Email,
    bool   IsVerifiedPlatformUser
);
