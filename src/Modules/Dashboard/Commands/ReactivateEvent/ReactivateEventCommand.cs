using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;

namespace AmarTools.Modules.Dashboard.Commands.ReactivateEvent;

/// <summary>
/// Moves an archived event back to active status.
/// Fails if the user already has 3 active events.
/// </summary>
/// <param name="EventId">The archived event to reactivate.</param>
public sealed record ReactivateEventCommand(Guid EventId) : IRequest<Result<EventSummaryDto>>;
