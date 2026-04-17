using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;

namespace AmarTools.Modules.Dashboard.Commands.UpdateContact;

/// <summary>
/// Updates a contact entry. For plain contacts: all fields editable.
/// For platform contacts: only <see cref="Notes"/> can be changed.
/// </summary>
public sealed record UpdateContactCommand(
    Guid    ContactId,
    string? Name,
    string? Email,
    string? Phone,
    string? Notes
) : IRequest<Result<ContactDto>>;
