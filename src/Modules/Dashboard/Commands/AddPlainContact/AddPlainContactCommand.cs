using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.Dashboard.Contracts;
using MediatR;

namespace AmarTools.Modules.Dashboard.Commands.AddPlainContact;

/// <summary>
/// Adds an external person (non-platform) to the current user's Contact Book.
/// </summary>
public sealed record AddPlainContactCommand(
    string  Name,
    string  Email,
    string? Phone,
    string? Notes
) : IRequest<Result<ContactDto>>;
