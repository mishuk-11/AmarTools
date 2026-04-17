using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using AmarTools.Modules.CertificateGenerator.Services;

namespace AmarTools.Modules.CertificateGenerator;

/// <summary>
/// Registers all Certificate Generator module services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCertificateGeneratorModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<ICertificateDatasetParser, CsvCertificateDatasetParser>();

        return services;
    }
}
