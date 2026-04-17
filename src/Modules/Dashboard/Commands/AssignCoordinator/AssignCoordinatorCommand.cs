using AmarTools.BuildingBlocks.Common;
using AmarTools.Domain.Enums;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;

namespace AmarTools.Modules.Dashboard.Commands.AssignCoordinator;

/// <summary>
/// Assigns a contact from the current user's Contact Book as a coordinator
/// on one of their events.
///
/// The contact must be a verified platform user (<c>IsPlatformUser = true</c>).
/// Plain contacts (external people without an AmarTools account) cannot be
/// assigned as coordinators because they have no login identity.
/// </summary>
public sealed record AssignCoordinatorCommand(
    Guid                  EventId,
    Guid                  ContactId,
    CoordinatorRole       Role,
    IEnumerable<string>?  Permissions
) : IRequest<Result<CoordinatorDto>>;
