namespace AmarTools.BuildingBlocks.Interfaces;

/// <summary>
/// Abstracts the resolution of the currently authenticated user's identity.
/// Implemented in <c>AmarTools.Infrastructure</c> using <c>IHttpContextAccessor</c>
/// and JWT claims. All modules use this interface — never HttpContext directly.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// The <see cref="Guid"/> of the authenticated user parsed from the JWT <c>sub</c> claim.
    /// Returns <c>null</c> for unauthenticated requests (public guest endpoints).
    /// </summary>
    Guid? UserId { get; }

    /// <summary>The user's email address from JWT claims. Null on unauthenticated requests.</summary>
    string? Email { get; }

    /// <summary><c>true</c> when a valid, non-expired JWT is present in the request.</summary>
    bool IsAuthenticated { get; }
}
