using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmarTools.Web.Controllers;

/// <summary>
/// Serves Razor views. Auth is enforced client-side via localStorage JWT;
/// server-side [Authorize] would block navigation requests that carry no Bearer header.
/// </summary>
[AllowAnonymous]
public sealed class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View();

    [HttpGet("/dashboard")]
    public IActionResult Dashboard() => View();

    [HttpGet("/admin")]
    public IActionResult Admin() => View();

    [HttpGet("/error")]
    public IActionResult Error() => View();

    /// <summary>
    /// Guest photo frame kiosk — standalone page, no auth required.
    /// The slug identifies the published photo frame event.
    /// </summary>
    [HttpGet("/kiosk/{slug}")]
    public IActionResult Kiosk(string slug) => View("~/Views/Kiosk/Index.cshtml", model: slug);
}
