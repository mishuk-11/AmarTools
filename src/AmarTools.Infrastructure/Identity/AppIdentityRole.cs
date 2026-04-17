using Microsoft.AspNetCore.Identity;

namespace AmarTools.Infrastructure.Identity;

/// <summary>
/// Application role entity used by ASP.NET Core Identity.
/// Roles (e.g. "Owner", "Admin") are platform-wide;
/// event-level access is controlled by <see cref="AmarTools.Domain.Entities.EventCoordinator"/>.
/// </summary>
public sealed class AppIdentityRole : IdentityRole<Guid>
{
    public AppIdentityRole() { }
    public AppIdentityRole(string roleName) : base(roleName) { }
}
