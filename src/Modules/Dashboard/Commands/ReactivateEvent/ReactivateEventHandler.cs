using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Commands.ReactivateEvent;

internal sealed class ReactivateEventHandler
    : IRequestHandler<ReactivateEventCommand, Result<EventSummaryDto>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _uow;

    public ReactivateEventHandler(
        AppDbContext db,
        ICurrentUserService currentUser,
        IUnitOfWork uow)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
    }

    public async Task<Result<EventSummaryDto>> Handle(
        ReactivateEventCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var @event = await _db.Events
            .Include(e => e.Tools)
            .FirstOrDefaultAsync(e => e.Id == command.EventId, ct);

        if (@event is null)
            return Error.NotFound("Event.NotFound", "Event not found.");

        if (@event.OwnerId != userId)
            return Error.Forbidden("Event.Forbidden", "You do not own this event.");

        // Current active count needed by the domain rule
        var activeCount = await _db.Events
            .CountAsync(e => e.OwnerId == userId && e.Status == EventStatus.Active, ct);

        var result = @event.Reactivate(activeCount);
        if (result.IsFailure) return result.Error;

        await _uow.SaveChangesAsync(ct);
        return @event.ToSummaryDto();
    }
}
