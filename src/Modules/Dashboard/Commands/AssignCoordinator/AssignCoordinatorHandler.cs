using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Commands.AssignCoordinator;

internal sealed class AssignCoordinatorHandler
    : IRequestHandler<AssignCoordinatorCommand, Result<CoordinatorDto>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _uow;

    public AssignCoordinatorHandler(
        AppDbContext db, ICurrentUserService currentUser, IUnitOfWork uow)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
    }

    public async Task<Result<CoordinatorDto>> Handle(
        AssignCoordinatorCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var ownerId = _currentUser.UserId.Value;

        // ── 1. Verify the event belongs to the caller ─────────────────────────
        var @event = await _db.Events
            .FirstOrDefaultAsync(e => e.Id == command.EventId, ct);

        if (@event is null)
            return Error.NotFound("Event.NotFound", "Event not found.");

        if (@event.OwnerId != ownerId)
            return Error.Forbidden("Event.Forbidden", "You do not own this event.");

        if (@event.Status != EventStatus.Active)
            return Error.Failure("Event.NotActive",
                "Coordinators can only be assigned to active events.");

        // ── 2. Verify the contact is in the caller's Contact Book ─────────────
        var contact = await _db.ContactBookEntries
            .FirstOrDefaultAsync(c => c.Id       == command.ContactId
                                   && c.OwnerId  == ownerId, ct);

        if (contact is null)
            return Error.NotFound("Contact.NotFound",
                "Contact not found in your contact book.");

        // Plain contacts have no platform identity — they cannot log in
        if (!contact.IsPlatformUser || contact.LinkedUserId is null)
            return Error.Validation("Contact.NotPlatformUser",
                "Only verified AmarTools users (platform contacts) can be assigned as coordinators.");

        var coordinatorUserId = contact.LinkedUserId.Value;

        // Prevent the event owner from assigning themselves
        if (coordinatorUserId == ownerId)
            return Error.Validation("Coordinator.SelfAssign",
                "You cannot assign yourself as a coordinator on your own event.");

        // ── 3. Check for existing (possibly revoked) assignment ───────────────
        var existing = await _db.EventCoordinators
            .FirstOrDefaultAsync(ec => ec.EventId           == command.EventId
                                    && ec.CoordinatorUserId == coordinatorUserId, ct);

        EventCoordinator assignment;

        if (existing is not null)
        {
            if (existing.IsActive)
                return Error.Conflict("Coordinator.AlreadyAssigned",
                    "This user is already an active coordinator on this event.");

            // Re-activate a previously revoked assignment
            existing.Reinstate();
            existing.ChangeRole(command.Role);
            existing.SetPermissions(command.Permissions ?? []);
            assignment = existing;
        }
        else
        {
            assignment = EventCoordinator.Create(
                command.EventId,
                coordinatorUserId,
                command.Role,
                command.Permissions);

            _db.EventCoordinators.Add(assignment);
        }

        await _uow.SaveChangesAsync(ct);

        // ── 4. Load coordinator's display profile ─────────────────────────────
        var coordinatorUser = await _db.DomainUsers
            .FirstOrDefaultAsync(u => u.Id == coordinatorUserId, ct);

        return assignment.ToCoordinatorDto(coordinatorUser!);
    }
}
