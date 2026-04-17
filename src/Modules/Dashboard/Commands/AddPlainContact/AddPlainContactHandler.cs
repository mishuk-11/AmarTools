using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Commands.AddPlainContact;

internal sealed class AddPlainContactHandler
    : IRequestHandler<AddPlainContactCommand, Result<ContactDto>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _uow;

    public AddPlainContactHandler(
        AppDbContext db, ICurrentUserService currentUser, IUnitOfWork uow)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
    }

    public async Task<Result<ContactDto>> Handle(
        AddPlainContactCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;
        var email  = command.Email.Trim().ToLowerInvariant();

        // ── Prevent duplicate plain contacts (same email in same book) ─────────
        var duplicate = await _db.ContactBookEntries
            .AnyAsync(c => c.OwnerId == userId
                        && c.ContactEmail == email
                        && c.LinkedUserId == null, ct);

        if (duplicate)
            return Error.Conflict(
                "Contact.DuplicateEmail",
                "A contact with this email already exists in your contact book.");

        var contact = ContactBookEntry.CreatePlain(
            userId, command.Name, email, command.Phone, command.Notes);

        _db.ContactBookEntries.Add(contact);
        await _uow.SaveChangesAsync(ct);

        return contact.ToContactDto();
    }
}
