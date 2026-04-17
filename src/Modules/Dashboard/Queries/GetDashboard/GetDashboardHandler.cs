using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Queries.GetDashboard;

internal sealed class GetDashboardHandler
    : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardHandler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db          = db;
        _currentUser = currentUser;
    }

    public async Task<Result<DashboardDto>> Handle(
        GetDashboardQuery query, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        // ── Load events with their activated tools (single query) ─────────────
        var events = await _db.Events
            .AsNoTracking()
            .Include(e => e.Tools)
            .Where(e => e.OwnerId == userId &&
                        e.Status != EventStatus.Closed)   // closed events excluded from dashboard
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

        var activeEvents   = events.Where(e => e.Status == EventStatus.Active)
                                   .Select(e => e.ToSummaryDto())
                                   .ToList();

        var archivedEvents = events.Where(e => e.Status == EventStatus.Archived)
                                   .Select(e => e.ToSummaryDto())
                                   .ToList();

        // ── Load active subscriptions ─────────────────────────────────────────
        var subscriptions = await _db.Subscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId && !s.IsRevoked &&
                        (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow))
            .ToListAsync(ct);

        var subscribedTools = subscriptions
            .Select(s => new SubscribedToolDto(
                s.Id,
                s.ToolType,
                s.ToolType.ToDisplayName(),
                s.StartedAt,
                s.ExpiresAt))
            .ToList();

        var dashboard = new DashboardDto(
            activeEvents,
            archivedEvents,
            RemainingEventSlots: Event.MaxActiveEvents - activeEvents.Count,
            subscribedTools);

        return dashboard;
    }
}
