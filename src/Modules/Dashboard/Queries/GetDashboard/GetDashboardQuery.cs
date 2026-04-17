using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;

namespace AmarTools.Modules.Dashboard.Queries.GetDashboard;

/// <summary>
/// Returns the full dashboard payload for the authenticated user:
/// active events, archived events, remaining slots, and subscribed tools.
/// </summary>
public sealed record GetDashboardQuery : IRequest<Result<DashboardDto>>;
