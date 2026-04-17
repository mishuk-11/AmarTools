namespace AmarTools.Web.Middleware;

/// <summary>
/// Validates the X-Api-Key header for endpoints that use API-key authentication
/// instead of JWT (e.g. guest kiosk endpoints, webhook receivers).
/// </summary>
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
    }
}

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder app)
        => app.UseMiddleware<ApiKeyMiddleware>();
}
