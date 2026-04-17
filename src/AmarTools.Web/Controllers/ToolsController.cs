using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmarTools.Web.Controllers;

/// <summary>Serves Razor views for each tool module. Auth enforced client-side via Auth.guard().</summary>
[AllowAnonymous]
public sealed class ToolsController : Controller
{
    [HttpGet("/tools/events")]       public IActionResult Events()      => View();
    [HttpGet("/tools/contacts")]     public IActionResult Contacts()    => View();
    [HttpGet("/tools/photo-frame")]  public IActionResult PhotoFrame()  => View();
    [HttpGet("/tools/certificates")] public IActionResult Certificates() => View();
}
