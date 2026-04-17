using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using AmarTools.Infrastructure.Identity;
using AmarTools.BuildingBlocks.Interfaces;

namespace AmarTools.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by <c>dotnet ef</c> tooling (migrations, scaffolding).
/// This lets you run migration commands targeting the Infrastructure project alone,
/// without having to build or run the Web startup project.
///
/// Usage:
///   dotnet dotnet-ef migrations add InitialCreate \
///       --project src/AmarTools.Infrastructure \
///       --startup-project src/AmarTools.Infrastructure
/// </summary>
internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Walk up from the Infrastructure project dir to find appsettings.json
        var basePath = FindWebProjectRoot() ?? Directory.GetCurrentDirectory();

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=amartools;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        // Pass a no-op ICurrentUserService — audit stamps are not needed at design time
        return new AppDbContext(optionsBuilder.Options, new NoOpCurrentUserService());
    }

    /// <summary>
    /// Walks up the directory tree to find the AmarTools.Web folder
    /// (which contains appsettings.json) relative to this project.
    /// </summary>
    private static string? FindWebProjectRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

        // Look up to 6 levels for the src/AmarTools.Web folder
        for (var i = 0; i < 6; i++)
        {
            if (dir == null) break;

            var candidate = Path.Combine(dir.FullName, "src", "AmarTools.Web");
            if (Directory.Exists(candidate))
                return candidate;

            // Also try sibling AmarTools.Web folder
            candidate = Path.Combine(dir.FullName, "AmarTools.Web");
            if (Directory.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        return null;
    }

    // ── Design-time stub ─────────────────────────────────────────────────────

    private sealed class NoOpCurrentUserService : ICurrentUserService
    {
        public Guid?  UserId          => null;
        public string? Email          => null;
        public bool   IsAuthenticated => false;
    }
}
