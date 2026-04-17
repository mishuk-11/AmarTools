using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;

namespace AmarTools.Modules.Dashboard.Queries.GetContacts;

/// <summary>
/// Returns the current user's contact book, optionally filtered by type.
/// </summary>
/// <param name="PlatformOnly">
/// <c>true</c>  → return only verified platform user contacts.<br/>
/// <c>false</c> → return only plain contacts.<br/>
/// <c>null</c>  → return all contacts (default).
/// </param>
/// <param name="SearchTerm">Optional name/email substring filter.</param>
public sealed record GetContactsQuery(
    bool?   PlatformOnly = null,
    string? SearchTerm   = null
) : IRequest<Result<IReadOnlyList<ContactDto>>>;
