using AmarTools.BuildingBlocks.Common;
using MediatR;

namespace AmarTools.Modules.Dashboard.Commands.ArchiveEvent;

/// <summary>
/// Archives an active event, freeing up one slot against the 3-event limit.
/// The event and all its tool data are retained in read-only form.
/// </summary>
/// <param name="EventId">The event to archive.</param>
public sealed record ArchiveEventCommand(Guid EventId) : IRequest<Result>;
