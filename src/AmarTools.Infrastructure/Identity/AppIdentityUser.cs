using Microsoft.AspNetCore.Identity;

namespace AmarTools.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity user.
/// Uses the same <see cref="Guid"/> primary key as <c>AmarTools.Domain.Entities.ApplicationUser</c>
/// so both tables are joined by <c>Id</c> without a separate FK column.
///
/// Keep this class thin — all business logic lives in the domain entity.
/// </summary>
public sealed class AppIdentityUser : IdentityUser<Guid>
{
    /// <summary>
    /// Mirrors <c>ApplicationUser.FullName</c> for convenience in Identity claims.
    /// Kept in sync by the application service layer.
    /// </summary>
    public string FullName { get; set; } = string.Empty;
}
