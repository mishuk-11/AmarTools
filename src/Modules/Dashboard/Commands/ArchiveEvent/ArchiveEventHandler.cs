using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Enums;
using AmarTools.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Commands.ArchiveEvent;

internal sealed class ArchiveEventHandler : IRequestHandler<ArchiveEventCommand, Result>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _uow;

    public ArchiveEventHandler(
        AppDbContext db,
        ICurrentUserService currentUser,
        IUnitOfWork uow)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
    }

    public async Task<Result> Handle(ArchiveEventCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        // ── Load event with ownership check ───────────────────────────────────
        var @event = await _db.Events
            .FirstOrDefaultAsync(e => e.Id == command.EventId, ct);

        if (@event is null)
            return Error.NotFound("Event.NotFound", "Event not found.");

        if (@event.OwnerId != userId)
            return Error.Forbidden("Event.Forbidden", "You do not own this event.");

        // ── Delegate business logic to domain entity ───────────────────────────
        var result = @event.Archive();
        if (result.IsFailure) return result;

        await _uow.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
