using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AmrTools.Data;
using AmrTools.Models;
using Microsoft.EntityFrameworkCore;

namespace AmrTools.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // --- REGISTER ---
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                
                var isFirstUser = !_context.Users.Any();

                var user = new User
                {
                    FullName = fullName,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                   
                    Role = isFirstUser ? "Admin" : "User", 
                    SubscriptionPlan = "Free"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View();
        }

       
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                
                bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                if (isValid)
                {
                    
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserName", user.FullName);
                    
                    HttpContext.Session.SetString("UserRole", user.Role ?? "User"); 
                    HttpContext.Session.SetString("UserPlan", user.SubscriptionPlan ?? "Free");
                    HttpContext.Session.SetString("UserId", user.Id.ToString());

                    
                    if (user.Role != null && user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    
                   return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Invalid email or password!";
            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}