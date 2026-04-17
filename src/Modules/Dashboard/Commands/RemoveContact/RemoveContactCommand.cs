using AmarTools.BuildingBlocks.Common;
using MediatR;

namespace AmarTools.Modules.Dashboard.Commands.RemoveContact;

/// <summary>
/// Permanently removes a contact from the current user's Contact Book.
/// If the contact is assigned as a coordinator on any event, those
/// assignments must be revoked first (enforced by the handler).
/// </summary>
public sealed record RemoveContactCommand(Guid ContactId) : IRequest<Result>;
