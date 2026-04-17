using AmarTools.BuildingBlocks.Common;
using MediatR;

namespace AmarTools.Modules.Dashboard.Commands.CloseEvent;

/// <summary>
/// Permanently closes an event. Closed events cannot be reactivated.
/// Data is retained for historical reference.
/// </summary>
/// <param name="EventId">The event to close.</param>
public sealed record CloseEventCommand(Guid EventId) : IRequest<Result>;
