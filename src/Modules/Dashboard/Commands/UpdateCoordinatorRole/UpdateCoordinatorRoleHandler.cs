using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Commands.UpdateCoordinatorRole;

internal sealed class UpdateCoordinatorRoleHandler
    : IRequestHandler<UpdateCoordinatorRoleCommand, Result<CoordinatorDto>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _uow;

    public UpdateCoordinatorRoleHandler(
        AppDbContext db, ICurrentUserService currentUser, IUnitOfWork uow)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
    }

    public async Task<Result<CoordinatorDto>> Handle(
        UpdateCoordinatorRoleCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var ownerId = _currentUser.UserId.Value;

        var assignment = await _db.EventCoordinators
            .Include(ec => ec.Event)
            .FirstOrDefaultAsync(ec => ec.Id == command.CoordinatorAssignmentId, ct);

        if (assignment is null)
            return Error.NotFound("Coordinator.NotFound", "Coordinator assignment not found.");

        if (assignment.Event.OwnerId != ownerId)
            return Error.Forbidden("Coordinator.Forbidden",
                "You do not own the event this coordinator is assigned to.");

        if (!assignment.IsActive)
            return Error.Failure("Coordinator.Revoked",
                "Cannot update a revoked coordinator assignment. Reinstate it first.");

        assignment.ChangeRole(command.NewRole);
        assignment.SetPermissions(command.NewPermissions ?? []);

        await _uow.SaveChangesAsync(ct);

        var coordinatorUser = await _db.DomainUsers
            .FirstOrDefaultAsync(u => u.Id == assignment.CoordinatorUserId, ct);

        return assignment.ToCoordinatorDto(coordinatorUser!);
    }
}
