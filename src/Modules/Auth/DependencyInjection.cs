using AmarTools.Modules.Auth.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AmarTools.Modules.Auth;

/// <summary>
/// Registers all Auth module services.
/// Called from <c>AmarTools.Web/Program.cs</c>:
/// <code>builder.Services.AddAuthModule();</code>
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}
