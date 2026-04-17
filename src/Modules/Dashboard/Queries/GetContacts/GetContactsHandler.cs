using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Dashboard.Queries.GetContacts;

internal sealed class GetContactsHandler
    : IRequestHandler<GetContactsQuery, Result<IReadOnlyList<ContactDto>>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;

    public GetContactsHandler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db          = db;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<ContactDto>>> Handle(
        GetContactsQuery query, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var dbQuery = _db.ContactBookEntries
            .AsNoTracking()
            .Include(c => c.LinkedUser)
            .Where(c => c.OwnerId == userId);

        // Platform-only / plain-only filter
        if (query.PlatformOnly == true)
            dbQuery = dbQuery.Where(c => c.LinkedUserId != null);
        else if (query.PlatformOnly == false)
            dbQuery = dbQuery.Where(c => c.LinkedUserId == null);

        // Search across name and email columns
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            dbQuery = dbQuery.Where(c =>
                (c.ContactName  != null && c.ContactName.ToLower().Contains(term))  ||
                (c.ContactEmail != null && c.ContactEmail.ToLower().Contains(term)) ||
                (c.LinkedUser   != null && (
                    c.LinkedUser.FullName.ToLower().Contains(term) ||
                    c.LinkedUser.Email.ToLower().Contains(term))));
        }

        var contacts = await dbQuery
            .OrderBy(c => c.LinkedUserId == null ? c.ContactName : c.LinkedUser!.FullName)
            .ToListAsync(ct);

        return contacts
            .Select(c => c.IsPlatformUser
                ? c.ToContactDto(c.LinkedUser)
                : c.ToContactDto())
            .ToList();
    }
}
