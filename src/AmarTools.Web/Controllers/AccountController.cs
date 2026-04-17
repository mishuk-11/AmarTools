using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmarTools.Web.Controllers;

/// <summary>
/// Serves the Login and Register Razor views.
/// All actions are anonymous — auth happens client-side via the API.
/// </summary>
[AllowAnonymous]
public sealed class AccountController : Controller
{
    // Note: already-authenticated checks happen client-side via Auth.guard() in JS.
    // Server-side User.IsInRole() does not work here because auth is JWT/localStorage-based.

    [HttpGet("/login")]
    public IActionResult Login() => View();

    [HttpGet("/register")]
    public IActionResult Register() => View();
}
