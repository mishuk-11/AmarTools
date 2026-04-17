namespace AmarTools.Modules.Auth.Contracts;

/// <summary>
/// Returned on successful login or registration.
/// The front-end stores <see cref="AccessToken"/> and passes it as
/// <c>Authorization: Bearer {AccessToken}</c> on every subsequent request.
/// </summary>
public sealed record AuthTokenDto(
    string   AccessToken,
    DateTime ExpiresAt,
    UserProfileDto User
);
