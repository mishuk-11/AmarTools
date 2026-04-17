using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.Auth.Contracts;
using MediatR;

namespace AmarTools.Modules.Auth.Commands.Login;

/// <summary>
/// Validates credentials and returns a signed JWT access token.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<AuthTokenDto>>;
