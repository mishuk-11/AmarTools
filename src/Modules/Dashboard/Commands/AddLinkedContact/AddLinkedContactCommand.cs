using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;

namespace AmarTools.Modules.Dashboard.Commands.AddLinkedContact;

/// <summary>
/// Links a verified AmarTools platform user to the current user's Contact Book.
/// The contact's display name and email are always sourced live from their profile.
/// </summary>
/// <param name="LinkedUserId">The Id of the verified AmarTools user to link.</param>
public sealed record AddLinkedContactCommand(
    Guid    LinkedUserId,
    string? Notes
) : IRequest<Result<ContactDto>>;
