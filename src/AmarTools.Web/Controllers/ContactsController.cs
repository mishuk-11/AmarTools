using AmarTools.Modules.Dashboard.Commands.AddLinkedContact;
using AmarTools.Modules.Dashboard.Commands.AddPlainContact;
using AmarTools.Modules.Dashboard.Commands.RemoveContact;
using AmarTools.Modules.Dashboard.Commands.UpdateContact;
using AmarTools.Modules.Dashboard.Contracts;
using AmarTools.Modules.Dashboard.Queries.GetContacts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmarTools.Web.Controllers;

/// <summary>
/// Manages the authenticated user's Contact Book.
/// All endpoints require a valid JWT bearer token.
/// </summary>
[Authorize]
public sealed class ContactsController : ApiControllerBase
{
    private readonly ISender _sender;

    public ContactsController(ISender sender) => _sender = sender;

    // ── GET /api/contacts ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns the current user's contact book.
    /// </summary>
    /// <param name="platformOnly">
    /// <c>true</c> = platform users only, <c>false</c> = plain contacts only, omit = all.
    /// </param>
    /// <param name="search">Optional name/email substring filter.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ContactDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetContacts(
        [FromQuery] bool?   platformOnly,
        [FromQuery] string? search,
        CancellationToken   ct)
    {
        var result = await _sender.Send(new GetContactsQuery(platformOnly, search), ct);
        return Ok(result);
    }

    // ── POST /api/contacts/plain ──────────────────────────────────────────────

    /// <summary>
    /// Adds an external person (no AmarTools account) to the contact book.
    /// </summary>
    [HttpPost("plain")]
    [ProducesResponseType(typeof(ContactDto), 201)]
    [ProducesResponseType(401)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> AddPlainContact(
        [FromBody] AddPlainContactRequest request,
        CancellationToken ct)
    {
        var command = new AddPlainContactCommand(
            request.Name, request.Email, request.Phone, request.Notes);

        var result = await _sender.Send(command, ct);
        return Created(result);
    }

    // ── POST /api/contacts/linked ─────────────────────────────────────────────

    /// <summary>
    /// Links a verified AmarTools platform user to the contact book.
    /// The linked user must have a verified account on the platform.
    /// </summary>
    [HttpPost("linked")]
    [ProducesResponseType(typeof(ContactDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> AddLinkedContact(
        [FromBody] AddLinkedContactRequest request,
        CancellationToken ct)
    {
        var command = new AddLinkedContactCommand(request.LinkedUserId, request.Notes);
        var result  = await _sender.Send(command, ct);
        return Created(result);
    }

    // ── PATCH /api/contacts/{contactId} ───────────────────────────────────────

    /// <summary>
    /// Updates a contact. For platform contacts only <c>notes</c> is editable.
    /// </summary>
    [HttpPatch("{contactId:guid}")]
    [ProducesResponseType(typeof(ContactDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateContact(
        Guid contactId,
        [FromBody] UpdateContactRequest request,
        CancellationToken ct)
    {
        var command = new UpdateContactCommand(
            contactId, request.Name, request.Email, request.Phone, request.Notes);

        var result = await _sender.Send(command, ct);
        return Ok(result);
    }

    // ── DELETE /api/contacts/{contactId} ──────────────────────────────────────

    /// <summary>
    /// Permanently removes a contact. Fails if the contact is an active coordinator
    /// on any of the caller's events — revoke those assignments first.
    /// </summary>
    [HttpDelete("{contactId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveContact(Guid contactId, CancellationToken ct)
    {
        var result = await _sender.Send(new RemoveContactCommand(contactId), ct);
        return NoContent(result);
    }
}

// ── Request models ────────────────────────────────────────────────────────────

public sealed record AddPlainContactRequest(
    string  Name,
    string  Email,
    string? Phone,
    string? Notes
);

public sealed record AddLinkedContactRequest(
    Guid    LinkedUserId,
    string? Notes
);

public sealed record UpdateContactRequest(
    string? Name,
    string? Email,
    string? Phone,
    string? Notes
);
