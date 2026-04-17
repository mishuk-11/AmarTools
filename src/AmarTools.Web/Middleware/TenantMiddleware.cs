namespace AmarTools.Web.Middleware;

/// <summary>
/// Resolves the current event tenant from the route or request header
/// and stores it in HttpContext.Items for downstream use.
/// Full implementation lives in AmarTools.Infrastructure.MultiTenancy.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
    }
}

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder app)
        => app.UseMiddleware<TenantMiddleware>();
}
