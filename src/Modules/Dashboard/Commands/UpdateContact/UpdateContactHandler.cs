using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Commands.UpdateContact;

internal sealed class UpdateContactHandler
    : IRequestHandler<UpdateContactCommand, Result<ContactDto>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _uow;

    public UpdateContactHandler(
        AppDbContext db, ICurrentUserService currentUser, IUnitOfWork uow)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
    }

    public async Task<Result<ContactDto>> Handle(
        UpdateContactCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var contact = await _db.ContactBookEntries
            .Include(c => c.LinkedUser)
            .FirstOrDefaultAsync(c => c.Id == command.ContactId, ct);

        if (contact is null)
            return Error.NotFound("Contact.NotFound", "Contact not found.");

        if (contact.OwnerId != userId)
            return Error.Forbidden("Contact.Forbidden", "You do not own this contact.");

        if (contact.IsPlatformUser)
        {
            // Platform contacts: only notes are editable
            contact.UpdateNotes(command.Notes);
        }
        else
        {
            // Plain contacts: full field edit — all non-null params applied
            var newName  = command.Name  ?? contact.ContactName  ?? string.Empty;
            var newEmail = command.Email ?? contact.ContactEmail ?? string.Empty;
            contact.UpdatePlainContact(newName, newEmail, command.Phone, command.Notes);
        }

        await _uow.SaveChangesAsync(ct);

        return contact.IsPlatformUser
            ? contact.ToContactDto(contact.LinkedUser)
            : contact.ToContactDto();
    }
}
