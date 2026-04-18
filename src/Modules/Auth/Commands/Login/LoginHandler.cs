using AmarTools.BuildingBlocks.Common;
using AmarTools.Infrastructure.Identity;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Auth.Contracts;
using AmarTools.Modules.Auth.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Auth.Commands.Login;

internal sealed class LoginHandler
    : IRequestHandler<LoginCommand, Result<AuthTokenDto>>
{
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly AppDbContext                 _db;
    private readonly ITokenService                _tokenService;

    public LoginHandler(
        UserManager<AppIdentityUser> userManager,
        AppDbContext                 db,
        ITokenService                tokenService)
    {
        _userManager  = userManager;
        _db           = db;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthTokenDto>> Handle(
        LoginCommand command, CancellationToken ct)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        // ── 1. Find Identity user ─────────────────────────────────────────────
        var identityUser = await _userManager.FindByEmailAsync(email);

        if (identityUser is null)
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

        // ── 2. Verify password ────────────────────────────────────────────────
        var passwordValid = await _userManager.CheckPasswordAsync(identityUser, command.Password);

        if (!passwordValid)
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

        // ── 3. Check account ban ──────────────────────────────────────────────
        if (identityUser.LockoutEnabled &&
            identityUser.LockoutEnd.HasValue &&
            identityUser.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            var banEnd      = identityUser.LockoutEnd.Value;
            var isPermanent = banEnd.Year >= 9999;
            var msg         = isPermanent
                ? "Your account has been permanently banned. Contact support."
                : $"Your account is banned until {banEnd.UtcDateTime:dd MMM yyyy HH:mm} UTC.";
            return Error.Forbidden("Auth.AccountBanned", msg);
        }

        // ── 4. Load domain user ───────────────────────────────────────────────
        var domainUser = await _db.DomainUsers
            .FirstOrDefaultAsync(u => u.Id == identityUser.Id, ct);

        if (domainUser is null)
            return Error.Failure("Auth.ProfileMissing", "User profile not found. Please contact support.");

        // ── 5. Get roles + issue token with role claims ───────────────────────
        var roles = await _userManager.GetRolesAsync(identityUser);
        var (token, expiresAt) = _tokenService.CreateToken(domainUser, roles);

        return new AuthTokenDto(
            token,
            expiresAt,
            new UserProfileDto(
                domainUser.Id,
                domainUser.FullName,
                domainUser.Email,
                domainUser.IsVerifiedPlatformUser));
    }
}
