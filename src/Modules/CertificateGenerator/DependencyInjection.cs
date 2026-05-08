using AmarTools.Modules.CertificateGenerator.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AmarTools.Modules.CertificateGenerator;

public static class DependencyInjection
{
    public static IServiceCollection AddCertificateGeneratorModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<ICertificateDatasetParser, CsvCertificateDatasetParser>();
        services.AddScoped<IPptxCertificateRenderer, PptxCertificateRenderer>();
        services.AddScoped<IPptxPlaceholderExtractor, PptxPlaceholderExtractor>();

        // Prefer LibreOffice (best fidelity) when installed; fall back to the built-in OpenXML renderer
        var libreOfficeConverter = new LibreOfficePdfConverter();
        services.AddSingleton<IPdfConverter>(
            libreOfficeConverter.IsAvailable
                ? (IPdfConverter)libreOfficeConverter
                : new OpenXmlPdfConverter());

        return services;
    }
}
