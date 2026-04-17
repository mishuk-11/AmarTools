using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Queries.GetEventCoordinators;

internal sealed class GetEventCoordinatorsHandler
    : IRequestHandler<GetEventCoordinatorsQuery, Result<IReadOnlyList<CoordinatorDto>>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;

    public GetEventCoordinatorsHandler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db          = db;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<CoordinatorDto>>> Handle(
        GetEventCoordinatorsQuery query, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        // Caller must own the event OR be a coordinator on it
        var eventExists = await _db.Events.AnyAsync(
            e => e.Id == query.EventId &&
                 (e.OwnerId == userId ||
                  _db.EventCoordinators.Any(
                      ec => ec.EventId == query.EventId &&
                            ec.CoordinatorUserId == userId &&
                            ec.IsActive)), ct);

        if (!eventExists)
            return Error.NotFound("Event.NotFound",
                "Event not found or you do not have access to it.");

        var dbQuery = _db.EventCoordinators
            .AsNoTracking()
            .Include(ec => ec.CoordinatorUser)
            .Where(ec => ec.EventId == query.EventId);

        if (query.ActiveOnly)
            dbQuery = dbQuery.Where(ec => ec.IsActive);

        var assignments = await dbQuery
            .OrderBy(ec => ec.CoordinatorUser.FullName)
            .ToListAsync(ct);

        return assignments
            .Select(ec => ec.ToCoordinatorDto(ec.CoordinatorUser))
            .ToList();
    }
}
