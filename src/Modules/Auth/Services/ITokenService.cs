using AmarTools.Domain.Entities;

namespace AmarTools.Modules.Auth.Services;

/// <summary>
/// Generates a signed JWT access token for an authenticated user.
/// Implemented by <see cref="JwtTokenService"/> using settings from
/// <c>appsettings.json → Jwt</c>.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates a signed JWT token containing the user's <c>sub</c>, <c>email</c>,
    /// and <c>name</c> claims.
    /// </summary>
    /// <param name="user">The domain user whose identity is being encoded.</param>
    /// <param name="roles">Platform roles to embed as <c>ClaimTypes.Role</c> claims.</param>
    /// <returns>The raw token string and its UTC expiry timestamp.</returns>
    (string Token, DateTime ExpiresAt) CreateToken(ApplicationUser user, IEnumerable<string> roles);
}
