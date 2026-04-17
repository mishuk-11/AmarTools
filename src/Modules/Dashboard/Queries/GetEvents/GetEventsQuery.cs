using AmarTools.BuildingBlocks.Common;
using AmarTools.Domain.Enums;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;

namespace AmarTools.Modules.Dashboard.Queries.GetEvents;

/// <summary>
/// Returns a filtered list of the current user's events.
/// Pass <c>null</c> to <paramref name="StatusFilter"/> to return all statuses.
/// </summary>
public sealed record GetEventsQuery(EventStatus? StatusFilter = null)
    : IRequest<Result<IReadOnlyList<EventSummaryDto>>>;
