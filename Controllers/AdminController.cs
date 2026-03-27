using Microsoft.AspNetCore.Mvc;
using AmrTools.Data;
using Microsoft.EntityFrameworkCore;
using AmrTools.Filters;
using AmrTools.Models;
namespace AmrTools.Controllers
{
    [AdminOnly]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {

            var users = await _context.Users.ToListAsync();
            ViewBag.TotalUsers = users.Count;
            ViewBag.ProUsers = users.Count(u => u.SubscriptionPlan != null &&
                                           u.SubscriptionPlan.Equals("Pro", StringComparison.OrdinalIgnoreCase));

            ViewBag.FreeUsers = users.Count(u => u.SubscriptionPlan == null ||
                                            u.SubscriptionPlan.Equals("Free", StringComparison.OrdinalIgnoreCase));

            return View(users);
        }
        [HttpPost]
        public async Task<IActionResult> MakePro(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.SubscriptionPlan = "Pro";

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> MakeFree(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.SubscriptionPlan = "Free";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Payments()
        {
            return View();
        }

        public IActionResult Tools()
        {
            var toolsPath = Path.Combine(Directory.GetCurrentDirectory(), "Tools");
            var installedTools = new List<string>();

            if (Directory.Exists(toolsPath))
            {
                installedTools = Directory.GetDirectories(toolsPath)
                                          .Select(Path.GetFileName)
                                          .ToList()!;
            }

            return View(installedTools);
        }

        public async Task<IActionResult> Events()
        {
            var events = await _context.Events
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();
            return View(events);
        }
    }
}