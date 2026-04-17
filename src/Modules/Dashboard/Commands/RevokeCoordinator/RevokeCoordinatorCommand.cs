using AmarTools.BuildingBlocks.Common;
using MediatR;

namespace AmarTools.Modules.Dashboard.Commands.RevokeCoordinator;

/// <summary>
/// Revokes a coordinator's access to an event (soft-delete — record is retained).
/// The coordinator can be reinstated later via <c>AssignCoordinatorCommand</c>.
/// </summary>
public sealed record RevokeCoordinatorCommand(Guid CoordinatorAssignmentId) : IRequest<Result>;
