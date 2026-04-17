using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Commands.CreateEvent;

internal sealed class CreateEventHandler
    : IRequestHandler<CreateEventCommand, Result<EventSummaryDto>>
{
    private readonly AppDbContext       _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork        _uow;

    public CreateEventHandler(
        AppDbContext db,
        ICurrentUserService currentUser,
        IUnitOfWork uow)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
    }

    public async Task<Result<EventSummaryDto>> Handle(
        CreateEventCommand command, CancellationToken ct)
    {
        // ── 1. Resolve caller ─────────────────────────────────────────────────
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in to create an event.");

        var userId = _currentUser.UserId.Value;

        var user = await _db.DomainUsers
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return Error.NotFound("User.NotFound", "User profile not found.");

        // ── 2. Enforce 3-active-event limit ───────────────────────────────────
        var activeCount = await _db.Events
            .CountAsync(e => e.OwnerId == userId && e.Status == EventStatus.Active, ct);

        if (activeCount >= Event.MaxActiveEvents)
            return Error.Failure(
                "Event.LimitReached",
                $"You can have at most {Event.MaxActiveEvents} active events. " +
                "Archive an existing event before creating a new one.");

        // ── 3. Create & persist ───────────────────────────────────────────────
        var newEvent = Event.Create(
            command.Name,
            userId,
            command.Description,
            command.EventDate,
            command.Venue);

        _db.Events.Add(newEvent);
        await _uow.SaveChangesAsync(ct);

        // ── 4. Return summary ─────────────────────────────────────────────────
        return newEvent.ToSummaryDto();
    }
}
