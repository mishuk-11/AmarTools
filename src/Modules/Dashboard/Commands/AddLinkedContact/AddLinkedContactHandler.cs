using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Commands.AddLinkedContact;

internal sealed class AddLinkedContactHandler
    : IRequestHandler<AddLinkedContactCommand, Result<ContactDto>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _uow;

    public AddLinkedContactHandler(
        AppDbContext db, ICurrentUserService currentUser, IUnitOfWork uow)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
    }

    public async Task<Result<ContactDto>> Handle(
        AddLinkedContactCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        if (command.LinkedUserId == userId)
            return Error.Validation(
                "Contact.SelfLink",
                "You cannot add yourself to your contact book.");

        // ── Verify the target user exists and is a verified platform user ──────
        var linkedUser = await _db.DomainUsers
            .FirstOrDefaultAsync(u => u.Id == command.LinkedUserId
                                   && u.IsVerifiedPlatformUser, ct);

        if (linkedUser is null)
            return Error.NotFound(
                "Contact.UserNotFound",
                "No verified AmarTools user found with that ID.");

        // ── Prevent duplicate links ────────────────────────────────────────────
        var alreadyLinked = await _db.ContactBookEntries
            .AnyAsync(c => c.OwnerId    == userId
                        && c.LinkedUserId == command.LinkedUserId, ct);

        if (alreadyLinked)
            return Error.Conflict(
                "Contact.AlreadyLinked",
                "This user is already in your contact book.");

        var contact = ContactBookEntry.CreateLinked(userId, command.LinkedUserId, command.Notes);

        _db.ContactBookEntries.Add(contact);
        await _uow.SaveChangesAsync(ct);

        return contact.ToContactDto(linkedUser);
    }
}
