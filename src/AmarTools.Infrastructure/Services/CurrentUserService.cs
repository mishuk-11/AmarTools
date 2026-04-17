using AmarTools.BuildingBlocks.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AmarTools.Infrastructure.Services;

/// <summary>
/// Resolves the currently authenticated user from the JWT claims
/// present in the HTTP request context.
///
/// The JWT token is validated upstream by <c>AddJwtBearer</c> middleware in the
/// Web project. This service only reads the already-validated claims.
/// </summary>
internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    /// <inheritdoc />
    public string? Email
        => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

    /// <inheritdoc />
    public bool IsAuthenticated
        => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
