using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Queries.GetEvents;

internal sealed class GetEventsHandler
    : IRequestHandler<GetEventsQuery, Result<IReadOnlyList<EventSummaryDto>>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;

    public GetEventsHandler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db          = db;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<EventSummaryDto>>> Handle(
        GetEventsQuery query, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var dbQuery = _db.Events
            .AsNoTracking()
            .Include(e => e.Tools)
            .Where(e => e.OwnerId == userId);

        if (query.StatusFilter.HasValue)
            dbQuery = dbQuery.Where(e => e.Status == query.StatusFilter.Value);

        var events = await dbQuery
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

        return events.Select(e => e.ToSummaryDto()).ToList();
    }
}
