using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Commands.RevokeCoordinator;

internal sealed class RevokeCoordinatorHandler : IRequestHandler<RevokeCoordinatorCommand, Result>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _uow;

    public RevokeCoordinatorHandler(
        AppDbContext db, ICurrentUserService currentUser, IUnitOfWork uow)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
    }

    public async Task<Result> Handle(RevokeCoordinatorCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var ownerId = _currentUser.UserId.Value;

        var assignment = await _db.EventCoordinators
            .Include(ec => ec.Event)
            .FirstOrDefaultAsync(ec => ec.Id == command.CoordinatorAssignmentId, ct);

        if (assignment is null)
            return Error.NotFound("Coordinator.NotFound", "Coordinator assignment not found.");

        // Only the event owner can revoke
        if (assignment.Event.OwnerId != ownerId)
            return Error.Forbidden("Coordinator.Forbidden",
                "You do not own the event this coordinator is assigned to.");

        var result = assignment.Revoke();
        if (result.IsFailure) return result;

        await _uow.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
