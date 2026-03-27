using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AmrTools.Models;
using AmrTools.Data;
using Microsoft.EntityFrameworkCore;
using AmrTools.Filters;

namespace AmrTools.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var role = HttpContext.Session.GetString("UserRole");

        if (role == "Admin")
        {
            return RedirectToAction("Index", "Admin");
        }
        return View();
    }
    public IActionResult Pricing()
    {
        return View();
    }


    public async Task<IActionResult> Upgrade()
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Auth");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            user.SubscriptionPlan = "Pro";
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("UserPlan", "Pro");
        }
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}