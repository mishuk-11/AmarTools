using Microsoft.AspNetCore.Http;

namespace AmarTools.Infrastructure.MultiTenancy;

/// <summary>
/// Resolves the current "tenant" (Event) from the HTTP request.
///
/// In AmarTools, multi-tenancy is event-scoped, not account-scoped.
/// The active event ID is passed by clients as a route segment or header:
///   • Route parameter:  /api/events/{eventId}/...
///   • Header fallback:  X-Event-Id: {guid}
///
/// The resolved <see cref="CurrentEventId"/> is consumed by module services
/// to scope their queries to the correct workspace.
/// </summary>
public sealed class TenantResolver
{
    private const string EventIdHeader = "X-Event-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// The event (tenant) ID extracted from the current request.
    /// Returns <c>null</c> for requests that are not event-scoped
    /// (e.g. authentication, dashboard root, subscription management).
    /// </summary>
    public Guid? CurrentEventId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is null) return null;

            // 1. Try route data first (preferred — part of the URL contract)
            if (context.Request.RouteValues.TryGetValue("eventId", out var routeValue)
                && Guid.TryParse(routeValue?.ToString(), out var routeEventId))
            {
                return routeEventId;
            }

            // 2. Fall back to custom header (used by non-REST contexts, e.g. SignalR)
            if (context.Request.Headers.TryGetValue(EventIdHeader, out var headerValue)
                && Guid.TryParse(headerValue, out var headerEventId))
            {
                return headerEventId;
            }

            return null;
        }
    }
}
