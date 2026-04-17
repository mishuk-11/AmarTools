using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Commands.ActivateTool;

internal sealed class ActivateToolHandler
    : IRequestHandler<ActivateToolCommand, Result<Guid>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _uow;

    public ActivateToolHandler(
        AppDbContext db, ICurrentUserService currentUser, IUnitOfWork uow)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
    }

    public async Task<Result<Guid>> Handle(
        ActivateToolCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var ev = await _db.Events
            .Include(e => e.Tools)
            .FirstOrDefaultAsync(e => e.Id == command.EventId, ct);

        if (ev is null)
            return Error.NotFound("Event.NotFound", "Event not found.");

        if (ev.OwnerId != userId)
            return Error.Forbidden("Event.Forbidden", "You do not own this event.");

        // Idempotent: return existing tool ID if already activated
        var existing = ev.Tools.FirstOrDefault(t => t.ToolType == command.ToolType);
        if (existing is not null)
            return existing.Id;

        var result = ev.ActivateTool(command.ToolType);
        if (result.IsFailure)
            return result.Error;

        await _uow.SaveChangesAsync(ct);
        return result.Value.Id;
    }
}
