using AmarTools.Modules.Auth.Commands.Login;
using AmarTools.Modules.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmarTools.Web.Controllers;

/// <summary>
/// Handles account registration and login.
/// Explicitly anonymous — must bypass the global FallbackPolicy.
/// </summary>
[AllowAnonymous]
public sealed class AuthController : ApiControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    // ── POST /api/auth/register ───────────────────────────────────────────────

    /// <summary>
    /// Creates a new AmarTools account and returns a JWT access token.
    /// The token can be used immediately — no email verification step in this release.
    /// </summary>
    /// <remarks>
    /// Returns 409 if the email is already registered.
    /// Returns 422 if the password does not meet complexity requirements.
    /// </remarks>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AmarTools.Modules.Auth.Contracts.AuthTokenDto), 201)]
    [ProducesResponseType(409)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var command = new RegisterCommand(
            request.FullName,
            request.Email,
            request.Password);

        var result = await _sender.Send(command, ct);
        return Created(result);
    }

    // ── POST /api/auth/login ──────────────────────────────────────────────────

    /// <summary>
    /// Validates credentials and returns a signed JWT access token.
    /// </summary>
    /// <remarks>
    /// Returns 401 with a generic message on invalid email or password
    /// to prevent user-enumeration attacks.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AmarTools.Modules.Auth.Contracts.AuthTokenDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result  = await _sender.Send(command, ct);
        return Ok(result);
    }
}

// ── Request models ────────────────────────────────────────────────────────────

/// <summary>Request body for <see cref="AuthController.Register"/>.</summary>
public sealed record RegisterRequest(
    string FullName,
    string Email,
    string Password
);

/// <summary>Request body for <see cref="AuthController.Login"/>.</summary>
public sealed record LoginRequest(
    string Email,
    string Password
);
