using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Security;
using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using AmarTools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AmarTools.Web.Controllers;

[Authorize]
public sealed class SubscriptionRequestController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public SubscriptionRequestController(AppDbContext db) => _db = db;

    // ── POST /api/subscriptionrequest ─────────────────────────────────────────
    /// <summary>
    /// Creates a pending subscription request for the authenticated user.
    /// packageDays must be 7, 30, or 90.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionRequestDto), 201)]
    [ProducesResponseType(422)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSubscriptionRequestBody body,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        // Only one pending request allowed at a time
        var hasPending = await _db.SubscriptionRequests
            .AnyAsync(r => r.UserId == userId
                        && r.Status == SubscriptionRequestStatus.Pending, ct);

        if (hasPending)
            return Conflict(new ProblemDetails
            {
                Title  = "SubscriptionRequest.AlreadyPending",
                Detail = "You already have a pending subscription request. Please wait for admin review."
            });

        var result = SubscriptionRequest.Create(userId, body.PackageDays);
        if (result.IsFailure) return UnprocessableEntity(new ProblemDetails
        {
            Title  = result.Error.Code,
            Detail = result.Error.Description
        });

        _db.SubscriptionRequests.Add(result.Value);
        await _db.SaveChangesAsync(ct);

        return StatusCode(201, SubscriptionRequestDto.From(result.Value));
    }

    // ── GET /api/subscriptionrequest/admin ────────────────────────────────────
    /// <summary>Lists pending subscription requests (admin only).</summary>
    [HttpGet("admin")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionRequestDto>), 200)]
    public async Task<IActionResult> ListPending(CancellationToken ct)
    {
        var requests = await _db.SubscriptionRequests
            .Include(r => r.User)
            .Where(r => r.Status == SubscriptionRequestStatus.Pending)
            .OrderBy(r => r.RequestedAt)
            .Select(r => SubscriptionRequestDto.From(r))
            .ToListAsync(ct);

        return base.Ok(requests);
    }

    // ── PUT /api/subscriptionrequest/{id}/approve ─────────────────────────────
    [HttpPut("{id:guid}/approve")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ReviewBody body,
        CancellationToken ct)
    {
        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty) return Unauthorized();

        var request = await _db.SubscriptionRequests.FindAsync([id], ct);
        if (request is null) return NotFound();

        var result = request.Approve(adminId, body.Notes);
        if (result.IsFailure)
            return Conflict(new ProblemDetails { Title = result.Error.Code, Detail = result.Error.Description });

        // Create subscriptions for all tool types
        var expiresAt = DateTime.UtcNow.AddDays(request.PackageDays);
        foreach (var tool in Enum.GetValues<ToolType>())
        {
            var subscription = Subscription.Create(request.UserId, tool, expiresAt);
            _db.Subscriptions.Add(subscription);
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── PUT /api/subscriptionrequest/{id}/reject ──────────────────────────────
    [HttpPut("{id:guid}/reject")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] ReviewBody body,
        CancellationToken ct)
    {
        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty) return Unauthorized();

        var request = await _db.SubscriptionRequests.FindAsync([id], ct);
        if (request is null) return NotFound();

        var result = request.Reject(adminId, body.Notes);
        if (result.IsFailure)
            return Conflict(new ProblemDetails { Title = result.Error.Code, Detail = result.Error.Description });

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

// ── Request / Response models ─────────────────────────────────────────────────

public sealed record CreateSubscriptionRequestBody(int PackageDays);

public sealed record ReviewBody(string? Notes = null);

public sealed record SubscriptionRequestDto(
    Guid    Id,
    Guid    UserId,
    string  UserName,
    string  UserEmail,
    int     PackageDays,
    string  Status,
    DateTime RequestedAt,
    DateTime? ReviewedAt,
    string? AdminNotes)
{
    public static SubscriptionRequestDto From(SubscriptionRequest r) => new(
        r.Id,
        r.UserId,
        r.User?.FullName ?? string.Empty,
        r.User?.Email    ?? string.Empty,
        r.PackageDays,
        r.Status.ToString(),
        r.RequestedAt,
        r.ReviewedAt,
        r.AdminNotes);
}
