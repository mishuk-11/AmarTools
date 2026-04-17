using AmarTools.BuildingBlocks.Security;
using AmarTools.Domain.Enums;
using AmarTools.Infrastructure.Identity;
using AmarTools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Web.Controllers;

/// <summary>
/// Admin-only management endpoints.
/// </summary>
[Authorize(Policy = "AdminOnly")]
[Route("api/admin")]
[ApiController]
public sealed class AdminController : ApiControllerBase
{
    private readonly AppDbContext              _db;
    private readonly UserManager<AppIdentityUser> _userManager;

    public AdminController(AppDbContext db, UserManager<AppIdentityUser> userManager)
    {
        _db          = db;
        _userManager = userManager;
    }

    // ── GET /api/admin/stats ──────────────────────────────────────────────────
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var totalUsers        = await _db.DomainUsers.CountAsync(ct);
        var totalEvents       = await _db.Events.CountAsync(ct);
        var pendingRequests   = await _db.SubscriptionRequests
                                         .CountAsync(r => r.Status == Domain.Enums.SubscriptionRequestStatus.Pending, ct);
        var subscribedUsers   = await _db.Subscriptions
                                         .Where(s => !s.IsRevoked && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow))
                                         .Select(s => s.UserId)
                                         .Distinct()
                                         .CountAsync(ct);

        return base.Ok(new { totalUsers, totalEvents, pendingRequests, subscribedUsers });
    }

    // ── GET /api/admin/events ─────────────────────────────────────────────────
    [HttpGet("events")]
    public async Task<IActionResult> GetAllEvents(CancellationToken ct)
    {
        var events = await _db.Events
            .Include(e => e.Owner)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.Venue,
                EventDate   = e.EventDate.HasValue ? e.EventDate.Value.ToString("yyyy-MM-dd") : null,
                Status      = e.Status.ToString(),
                OwnerName   = e.Owner.FullName,
                OwnerEmail  = e.Owner.Email,
                CreatedAt   = e.CreatedAt
            })
            .ToListAsync(ct);

        return base.Ok(events);
    }

    // ── GET /api/admin/users ──────────────────────────────────────────────────
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct)
    {
        var users = await _db.DomainUsers
            .OrderBy(u => u.FullName)
            .Select(u => new { u.Id, u.FullName, u.Email, u.CreatedAt })
            .ToListAsync(ct);

        var activeSubs = await _db.Subscriptions
            .Where(s => !s.IsRevoked && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow))
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, ExpiresAt = g.Min(s => s.ExpiresAt) })
            .ToListAsync(ct);

        var subMap = activeSubs.ToDictionary(s => s.UserId);

        // Identity fields: lockout state and roles — single batch queries
        var identityUsers = await _userManager.Users
            .Select(iu => new { iu.Id, iu.LockoutEnd })
            .ToListAsync(ct);
        var identityMap = identityUsers.ToDictionary(iu => iu.Id);

        var userRoles = await _db.UserRoles
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .ToListAsync(ct);
        var roleMap = userRoles.GroupBy(ur => ur.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Name!).ToList());

        var result = users.Select(u =>
        {
            subMap.TryGetValue(u.Id, out var sub);
            identityMap.TryGetValue(u.Id, out var iu);
            roleMap.TryGetValue(u.Id, out var roles);

            var isAdmin     = roles?.Contains(Roles.Admin) == true;
            var lockoutEnd  = iu?.LockoutEnd;
            var isBanned    = lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow;
            var isPermanent = lockoutEnd?.Year >= 9999;
            var banUntil    = isBanned && !isPermanent ? lockoutEnd?.UtcDateTime : (DateTime?)null;

            return (object)new
            {
                u.Id, u.FullName, u.Email, u.CreatedAt,
                HasSubscription = sub != null,
                SubExpiresAt    = sub?.ExpiresAt,
                IsAdmin         = isAdmin,
                IsBanned        = isBanned,
                BanIsPermanent  = isPermanent,
                BanUntil        = banUntil
            };
        });

        return base.Ok(result);
    }

    // ── POST /api/admin/users/{id}/make-admin ─────────────────────────────────
    [HttpPost("users/{userId:guid}/make-admin")]
    public async Task<IActionResult> MakeAdmin(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return NotFound();

        if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
            await _userManager.AddToRoleAsync(user, Roles.Admin);

        return base.Ok(new { message = "User promoted to Admin." });
    }

    // ── POST /api/admin/users/{id}/remove-admin ───────────────────────────────
    [HttpPost("users/{userId:guid}/remove-admin")]
    public async Task<IActionResult> RemoveAdmin(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return NotFound();

        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            await _userManager.RemoveFromRoleAsync(user, Roles.Admin);

        return base.Ok(new { message = "Admin role removed." });
    }

    // ── POST /api/admin/users/{id}/ban ────────────────────────────────────────
    [HttpPost("users/{userId:guid}/ban")]
    public async Task<IActionResult> BanUser(Guid userId, [FromBody] BanRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return NotFound();

        user.LockoutEnabled = true;
        user.LockoutEnd     = request.Days.HasValue
            ? DateTimeOffset.UtcNow.AddDays(request.Days.Value)
            : DateTimeOffset.MaxValue; // permanent

        await _userManager.UpdateAsync(user);
        return base.Ok(new { message = request.Days.HasValue ? $"User banned for {request.Days} days." : "User permanently banned." });
    }

    // ── POST /api/admin/users/{id}/unban ──────────────────────────────────────
    [HttpPost("users/{userId:guid}/unban")]
    public async Task<IActionResult> UnbanUser(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return NotFound();

        await _userManager.SetLockoutEndDateAsync(user, null);
        return base.Ok(new { message = "User unbanned." });
    }
}

public sealed record BanRequest(int? Days);
