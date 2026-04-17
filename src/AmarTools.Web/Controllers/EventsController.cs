using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Dashboard.Commands.ArchiveEvent;
using AmarTools.Modules.Dashboard.Commands.CloseEvent;
using AmarTools.Modules.Dashboard.Commands.CreateEvent;
using AmarTools.Modules.Dashboard.Commands.ReactivateEvent;
using AmarTools.Modules.Dashboard.Queries.GetDashboard;
using AmarTools.Modules.Dashboard.Queries.GetEvents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Web.Controllers;

/// <summary>
/// Manages the authenticated user's events (workspaces).
/// All endpoints require a valid JWT bearer token.
/// </summary>
[Authorize]
public sealed class EventsController : ApiControllerBase
{
    private readonly ISender             _sender;
    private readonly AppDbContext        _db;
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _currentUser;

    public EventsController(
        ISender sender,
        AppDbContext db,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _sender      = sender;
        _db          = db;
        _uow         = uow;
        _currentUser = currentUser;
    }

    // ── GET /api/events/dashboard ─────────────────────────────────────────────

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await _sender.Send(new GetDashboardQuery(), ct);
        return Ok(result);
    }

    // ── GET /api/events ───────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetEvents(
        [FromQuery] EventStatus? status, CancellationToken ct)
    {
        var result = await _sender.Send(new GetEventsQuery(status), ct);
        return Ok(result);
    }

    // ── POST /api/events ──────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> CreateEvent(
        [FromBody] CreateEventRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new CreateEventCommand(
            request.Name, request.Description, request.EventDate, request.Venue), ct);
        return Created(result);
    }

    // ── PATCH /api/events/{eventId}/archive ───────────────────────────────────

    [HttpPatch("{eventId:guid}/archive")]
    public async Task<IActionResult> ArchiveEvent(Guid eventId, CancellationToken ct)
    {
        var result = await _sender.Send(new ArchiveEventCommand(eventId), ct);
        return NoContent(result);
    }

    // ── PATCH /api/events/{eventId}/reactivate ────────────────────────────────

    [HttpPatch("{eventId:guid}/reactivate")]
    public async Task<IActionResult> ReactivateEvent(Guid eventId, CancellationToken ct)
    {
        var result = await _sender.Send(new ReactivateEventCommand(eventId), ct);
        return Ok(result);
    }

    // ── PATCH /api/events/{eventId}/close ─────────────────────────────────────

    [HttpPatch("{eventId:guid}/close")]
    public async Task<IActionResult> CloseEvent(Guid eventId, CancellationToken ct)
    {
        var result = await _sender.Send(new CloseEventCommand(eventId), ct);
        return NoContent(result);
    }

    // ── PATCH /api/events/{eventId} ──────────────────────────────────────────

    [HttpPatch("{eventId:guid}")]
    public async Task<IActionResult> UpdateEvent(
        Guid eventId, [FromBody] UpdateEventRequest request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Unauthorized();

        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct);
        if (ev is null) return NotFound();
        if (ev.OwnerId != _currentUser.UserId.Value)
            return StatusCode(403, new ProblemDetails { Title = "Event.Forbidden", Detail = "You do not own this event." });

        ev.UpdateDetails(request.Name, request.Description, request.EventDate, request.Venue);

        try { await _uow.SaveChangesAsync(ct); }
        catch (DbUpdateException ex)
        {
            var msg = ex.Message;
            for (var inner = ex.InnerException; inner is not null; inner = inner.InnerException) msg = inner.Message;
            return BadRequest(new ProblemDetails { Detail = msg });
        }
        return Ok(new { ev.Id, ev.Name });
    }

    // ── POST /api/events/{eventId}/activate-tool/{toolType} ───────────────────

    /// <summary>
    /// Activates a tool inside an event. Idempotent: returns the EventTool Id
    /// whether the tool was just created or already existed.
    /// </summary>
    [HttpPost("{eventId:guid}/activate-tool/{toolType:int}")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ActivateTool(
        Guid eventId, int toolType, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Unauthorized();

        var userId = _currentUser.UserId.Value;
        var tool   = (ToolType)toolType;

        // Load event only (no collection Include — avoids backing-field tracking issues)
        var ev = await _db.Events
            .FirstOrDefaultAsync(e => e.Id == eventId, ct);

        if (ev is null)
            return NotFound(new ProblemDetails
            {
                Title  = "Event.NotFound",
                Detail = "Event not found."
            });

        if (ev.OwnerId != userId)
            return StatusCode(403, new ProblemDetails
            {
                Title  = "Event.Forbidden",
                Detail = "You do not own this event."
            });

        if (ev.Status != EventStatus.Active)
            return BadRequest(new ProblemDetails
            {
                Title  = "Event.NotActive",
                Detail = "Tools can only be activated on an active event."
            });

        // Idempotent: query EventTools directly instead of via aggregate collection
        var existing = await _db.EventTools
            .FirstOrDefaultAsync(t => t.EventId == eventId && t.ToolType == tool, ct);

        if (existing is not null)
            return base.Ok(existing.Id);

        // Add directly to DbSet to avoid private-backing-field change-tracking issues
        var newTool = EventTool.Create(eventId, tool);
        _db.EventTools.Add(newTool);
        await _uow.SaveChangesAsync(ct);
        return base.Ok(newTool.Id);
    }
}

// ── Request models ────────────────────────────────────────────────────────────

public sealed record CreateEventRequest(
    string    Name,
    string?   Description,
    DateOnly? EventDate,
    string?   Venue
);

public sealed record UpdateEventRequest(
    string    Name,
    string?   Description,
    DateOnly? EventDate,
    string?   Venue
);
