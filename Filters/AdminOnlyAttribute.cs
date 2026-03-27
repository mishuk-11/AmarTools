using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AmrTools.Filters
{
    
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
            base.OnActionExecuting(context);
        }
    }
}