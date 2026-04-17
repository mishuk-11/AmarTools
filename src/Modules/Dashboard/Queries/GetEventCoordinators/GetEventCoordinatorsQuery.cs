using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;

namespace AmarTools.Modules.Dashboard.Queries.GetEventCoordinators;

/// <summary>Returns all coordinator assignments for a specific event.</summary>
/// <param name="EventId">The event to query coordinators for.</param>
/// <param name="ActiveOnly">When <c>true</c>, returns only active (non-revoked) assignments.</param>
public sealed record GetEventCoordinatorsQuery(
    Guid EventId,
    bool ActiveOnly = true
) : IRequest<Result<IReadOnlyList<CoordinatorDto>>>;
