using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.Auth.Contracts;
using MediatR;

namespace AmarTools.Modules.Auth.Commands.Register;

/// <summary>
/// Creates a new AmarTools account and returns a JWT access token.
///
/// Side effects:
/// <list type="bullet">
///   <item>Creates a row in <c>identity_users</c> (ASP.NET Core Identity).</item>
///   <item>Creates a row in <c>users</c> (domain <c>ApplicationUser</c>).</item>
/// </list>
/// Both rows share the same <see cref="Guid"/> primary key.
/// </summary>
public sealed record RegisterCommand(
    string FullName,
    string Email,
    string Password
) : IRequest<Result<AuthTokenDto>>;
