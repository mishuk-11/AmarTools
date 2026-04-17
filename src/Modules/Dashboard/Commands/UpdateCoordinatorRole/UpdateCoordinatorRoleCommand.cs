using AmarTools.BuildingBlocks.Common;
using AmarTools.Domain.Enums;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;

namespace AmarTools.Modules.Dashboard.Commands.UpdateCoordinatorRole;

/// <summary>
/// Changes a coordinator's role and/or fine-grained permission set on an event.
/// </summary>
public sealed record UpdateCoordinatorRoleCommand(
    Guid                 CoordinatorAssignmentId,
    CoordinatorRole      NewRole,
    IEnumerable<string>? NewPermissions
) : IRequest<Result<CoordinatorDto>>;
