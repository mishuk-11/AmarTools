using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.BuildingBlocks.Security;
using AmarTools.Domain.Entities;
using AmarTools.Infrastructure.Identity;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.Auth.Contracts;
using AmarTools.Modules.Auth.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.Auth.Commands.Register;

internal sealed class RegisterHandler
    : IRequestHandler<RegisterCommand, Result<AuthTokenDto>>
{
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly AppDbContext                 _db;
    private readonly IUnitOfWork                  _uow;
    private readonly ITokenService                _tokenService;

    public RegisterHandler(
        UserManager<AppIdentityUser> userManager,
        AppDbContext                 db,
        IUnitOfWork                  uow,
        ITokenService                tokenService)
    {
        _userManager  = userManager;
        _db           = db;
        _uow          = uow;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthTokenDto>> Handle(
        RegisterCommand command, CancellationToken ct)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        // ── 1. Check for duplicate email ──────────────────────────────────────
        var exists = await _db.DomainUsers
            .AnyAsync(u => u.Email == email, ct);

        if (exists)
            return Error.Conflict(
                "Auth.EmailTaken",
                "An account with this email address already exists.");

        // ── 2. Create Identity user ───────────────────────────────────────────
        var identityUser = new AppIdentityUser
        {
            Id             = Guid.NewGuid(),
            UserName       = email,
            Email          = email,
            FullName       = command.FullName.Trim(),
            EmailConfirmed = true
        };

        var identityResult = await _userManager.CreateAsync(identityUser, command.Password);

        if (!identityResult.Succeeded)
        {
            var firstError = identityResult.Errors.First();
            return Error.Validation("Auth.PasswordInvalid", firstError.Description);
        }

        // ── 3. Assign default Owner role ──────────────────────────────────────
        await _userManager.AddToRoleAsync(identityUser, Roles.Owner);

        // ── 4. Create domain user (same Id) ───────────────────────────────────
        var domainUser = ApplicationUser.Create(command.FullName, email);
        SetEntityId(domainUser, identityUser.Id);

        _db.DomainUsers.Add(domainUser);
        await _uow.SaveChangesAsync(ct);

        // ── 5. Issue token with roles ─────────────────────────────────────────
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

    private static void SetEntityId<T>(T entity, Guid id)
    {
        var prop = typeof(AmarTools.BuildingBlocks.Domain.BaseEntity)
            .GetProperty(nameof(AmarTools.BuildingBlocks.Domain.BaseEntity.Id));
        prop?.SetValue(entity, id);
    }
}
