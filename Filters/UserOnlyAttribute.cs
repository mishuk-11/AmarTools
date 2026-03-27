using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AmrTools.Filters
{
    public class UserOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(role))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
            else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Index", "Admin", null);
            }
            base.OnActionExecuting(context);
        }
    }
}