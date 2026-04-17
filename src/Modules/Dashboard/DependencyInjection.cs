using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AmarTools.Modules.Dashboard;

/// <summary>
/// Registers all Dashboard module services with the DI container.
/// Called from <c>AmarTools.Web/Program.cs</c>:
/// <code>builder.Services.AddDashboardModule();</code>
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDashboardModule(this IServiceCollection services)
    {
        // Register all IRequestHandler<,> implementations in this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}
