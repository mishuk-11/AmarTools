using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;

namespace AmarTools.Modules.Dashboard.Commands.CreateEvent;

/// <summary>
/// Creates a new active event (workspace) for the current user.
///
/// Business rules enforced in the handler:
/// <list type="bullet">
///   <item>User must not already have 3 active events.</item>
///   <item>User must exist in the domain user table.</item>
/// </list>
/// </summary>
public sealed record CreateEventCommand(
    string    Name,
    string?   Description,
    DateOnly? EventDate,
    string?   Venue
) : IRequest<Result<EventSummaryDto>>;
