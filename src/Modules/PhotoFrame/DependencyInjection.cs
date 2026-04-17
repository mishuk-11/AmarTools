using AmarTools.Modules.PhotoFrame.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AmarTools.Modules.PhotoFrame;

/// <summary>
/// Registers all PhotoFrame module services.
/// Called from <c>AmarTools.Web/Program.cs</c>:
/// <code>builder.Services.AddPhotoFrameModule();</code>
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPhotoFrameModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<IImageService, ImageSharpService>();

        return services;
    }
}
