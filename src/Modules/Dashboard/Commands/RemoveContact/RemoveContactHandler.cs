using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Commands.RemoveContact;

internal sealed class RemoveContactHandler : IRequestHandler<RemoveContactCommand, Result>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _uow;

    public RemoveContactHandler(
        AppDbContext db, ICurrentUserService currentUser, IUnitOfWork uow)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
    }

    public async Task<Result> Handle(RemoveContactCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var contact = await _db.ContactBookEntries
            .FirstOrDefaultAsync(c => c.Id == command.ContactId, ct);

        if (contact is null)
            return Error.NotFound("Contact.NotFound", "Contact not found.");

        if (contact.OwnerId != userId)
            return Error.Forbidden("Contact.Forbidden", "You do not own this contact.");

        // ── Guard: block removal if active coordinator assignments exist ───────
        // LinkedUserId is null for plain contacts, so this check only fires for
        // platform users who may be coordinators on the owner's events.
        if (contact.LinkedUserId.HasValue)
        {
            var hasActiveAssignment = await _db.EventCoordinators
                .AnyAsync(ec => ec.CoordinatorUserId == contact.LinkedUserId.Value
                             && ec.IsActive
                             && _db.Events.Any(e => e.Id == ec.EventId && e.OwnerId == userId),
                         ct);

            if (hasActiveAssignment)
                return Error.Failure(
                    "Contact.ActiveCoordinator",
                    "This contact is an active coordinator on one or more of your events. " +
                    "Revoke their coordinator access first.");
        }

        _db.ContactBookEntries.Remove(contact);
        await _uow.SaveChangesAsync(ct);
        return Result.Ok;
    }
}
