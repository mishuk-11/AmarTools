using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AmarTools.Web.Controllers;

[Authorize]
[Route("api/users")]
[ApiController]
public sealed class UsersController : ControllerBase
{
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly ICurrentUserService          _currentUser;

    public UsersController(UserManager<AppIdentityUser> userManager, ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    // ── GET /api/users/me ─────────────────────────────────────────────────────
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId.ToString()!);
        if (user is null) return NotFound();

        var lockoutEnd  = user.LockoutEnd;
        var isBanned    = lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow;
        var isPermanent = lockoutEnd?.Year >= 9999;
        var banUntil    = isBanned && !isPermanent ? lockoutEnd?.UtcDateTime : (DateTime?)null;

        return Ok(new
        {
            IsBanned       = isBanned,
            BanIsPermanent = isPermanent,
            BanUntil       = banUntil
        });
    }
}
