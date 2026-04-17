using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Identity;
using AmarTools.Infrastructure.MultiTenancy;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Infrastructure.Persistence.Repositories;
using AmarTools.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AmarTools.Infrastructure;

/// <summary>
/// Registers all infrastructure services with the DI container.
/// Called once from <c>AmarTools.Web/Program.cs</c>:
/// <code>builder.Services.AddInfrastructure(builder.Configuration);</code>
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ──────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql
                    .MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                    .EnableRetryOnFailure(maxRetryCount: 3)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // ── Generic Repository ────────────────────────────────────────────────
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // ── ASP.NET Core Identity ─────────────────────────────────────────────
        services.AddIdentityCore<AppIdentityUser>(options =>
            {
                options.Password.RequiredLength         = 8;
                options.Password.RequireDigit           = true;
                options.Password.RequireUppercase       = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail         = true;
                options.SignIn.RequireConfirmedEmail    = true;
            })
            .AddRoles<AppIdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // ── Multi-Tenancy ─────────────────────────────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<TenantResolver>();

        // ── Current User ──────────────────────────────────────────────────────
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── File Storage (local dev — swap to cloud impl for production) ──────
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // ── Identity Seeder ───────────────────────────────────────────────────
        services.AddScoped<IdentitySeeder>();

        return services;
    }
}
