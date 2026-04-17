using AmarTools.Domain.Enums;
using AmarTools.Modules.Dashboard.Commands.AssignCoordinator;
using AmarTools.Modules.Dashboard.Commands.RevokeCoordinator;
using AmarTools.Modules.Dashboard.Commands.UpdateCoordinatorRole;
using AmarTools.Modules.Dashboard.Contracts;
using AmarTools.Modules.Dashboard.Queries.GetEventCoordinators;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmarTools.Web.Controllers;

/// <summary>
/// Manages coordinator (RBAC) assignments for a specific event.
/// Nested under events: /api/events/{eventId}/coordinators
/// </summary>
[Authorize]
[Route("api/events/{eventId:guid}/coordinators")]
[ApiController]
public sealed class CoordinatorsController : ApiControllerBase
{
    private readonly ISender _sender;

    public CoordinatorsController(ISender sender) => _sender = sender;

    // ── GET /api/events/{eventId}/coordinators ────────────────────────────────

    /// <summary>
    /// Returns all coordinator assignments for the specified event.
    /// Both the event owner and any active coordinator can call this.
    /// </summary>
    /// <param name="activeOnly">When <c>true</c> (default), only active assignments are returned.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CoordinatorDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCoordinators(
        Guid eventId,
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetEventCoordinatorsQuery(eventId, activeOnly), ct);
        return Ok(result);
    }

    // ── POST /api/events/{eventId}/coordinators ───────────────────────────────

    /// <summary>
    /// Assigns a platform contact from the caller's Contact Book as a coordinator.
    /// The contact must be a verified AmarTools user.
    /// If a revoked assignment already exists for this user, it is reinstated.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CoordinatorDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> AssignCoordinator(
        Guid eventId,
        [FromBody] AssignCoordinatorRequest request,
        CancellationToken ct)
    {
        var command = new AssignCoordinatorCommand(
            eventId,
            request.ContactId,
            request.Role,
            request.Permissions);

        var result = await _sender.Send(command, ct);
        return Created(result);
    }

    // ── PATCH /api/events/{eventId}/coordinators/{assignmentId}/role ──────────

    /// <summary>Changes the role and/or permissions of an existing coordinator assignment.</summary>
    [HttpPatch("{assignmentId:guid}/role")]
    [ProducesResponseType(typeof(CoordinatorDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateRole(
        Guid assignmentId,
        [FromBody] UpdateCoordinatorRoleRequest request,
        CancellationToken ct)
    {
        var command = new UpdateCoordinatorRoleCommand(
            assignmentId, request.Role, request.Permissions);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    // ── DELETE /api/events/{eventId}/coordinators/{assignmentId} ─────────────

    /// <summary>
    /// Revokes a coordinator's access to this event (soft-delete).
    /// The assignment record is retained and can be reinstated via POST.
    /// </summary>
    [HttpDelete("{assignmentId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RevokeCoordinator(
        Guid assignmentId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new RevokeCoordinatorCommand(assignmentId), ct);
        return NoContent(result);
    }
}

// ── Request models ────────────────────────────────────────────────────────────

public sealed record AssignCoordinatorRequest(
    Guid                  ContactId,
    CoordinatorRole       Role,
    IEnumerable<string>?  Permissions
);

public sealed record UpdateCoordinatorRoleRequest(
    CoordinatorRole      Role,
    IEnumerable<string>? Permissions
);
