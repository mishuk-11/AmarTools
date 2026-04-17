using AmarTools.BuildingBlocks.Security;
using AmarTools.Domain.Entities;
using AmarTools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AmarTools.Infrastructure.Identity;

/// <summary>
/// Seeds the required Identity roles and a default platform admin account.
/// Called once from <c>Program.cs</c> after the app is built.
/// </summary>
public sealed class IdentitySeeder
{
    private readonly RoleManager<AppIdentityRole>  _roleManager;
    private readonly UserManager<AppIdentityUser>  _userManager;
    private readonly AppDbContext                  _db;
    private readonly ILogger<IdentitySeeder>       _logger;

    // ── Default admin credentials ─────────────────────────────────────────────
    private const string AdminEmail    = "admin@amartools.com";
    private const string AdminPassword = "Admin@12345";
    private const string AdminFullName = "Platform Admin";

    public IdentitySeeder(
        RoleManager<AppIdentityRole> roleManager,
        UserManager<AppIdentityUser> userManager,
        AppDbContext                 db,
        ILogger<IdentitySeeder>      logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db          = db;
        _logger      = logger;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedAdminUserAsync();
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in Roles.All)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await _roleManager.CreateAsync(new AppIdentityRole(roleName));

            if (result.Succeeded)
                _logger.LogInformation("Seeded role: {Role}", roleName);
            else
                _logger.LogWarning("Failed to seed role {Role}: {Errors}",
                    roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    // ── Default admin user ────────────────────────────────────────────────────

    private async Task SeedAdminUserAsync()
    {
        var email = AdminEmail.ToLowerInvariant();

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
            return; // already seeded

        var id = Guid.NewGuid();

        var identityUser = new AppIdentityUser
        {
            Id             = id,
            UserName       = email,
            Email          = email,
            FullName       = AdminFullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(identityUser, AdminPassword);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to seed admin user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await _userManager.AddToRoleAsync(identityUser, Roles.Admin);

        // Create matching domain user
        var domainUser = ApplicationUser.Create(AdminFullName, email);
        SetEntityId(domainUser, id);
        _db.DomainUsers.Add(domainUser);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Seeded default admin user: {Email}", email);
    }

    private static void SetEntityId(ApplicationUser entity, Guid id)
    {
        var prop = typeof(AmarTools.BuildingBlocks.Domain.BaseEntity)
            .GetProperty(nameof(AmarTools.BuildingBlocks.Domain.BaseEntity.Id));
        prop?.SetValue(entity, id);
    }
}
