using AmarTools.BuildingBlocks.Domain;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AmarTools.Infrastructure.Persistence;

/// <summary>
/// The single EF Core <see cref="DbContext"/> for the entire AmarTools platform.
///
/// Inherits from <see cref="IdentityDbContext{TUser, TRole, TKey}"/> so that
/// ASP.NET Core Identity tables share the same database and can participate
/// in the same transactions as the domain tables.
///
/// Implements <see cref="IUnitOfWork"/> so module services can flush changes
/// without taking a direct dependency on EF Core.
/// </summary>
public sealed class AppDbContext
    : IdentityDbContext<AppIdentityUser, AppIdentityRole, Guid>, IUnitOfWork
{
    private readonly ICurrentUserService _currentUser;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    // ── DbSets ────────────────────────────────────────────────────────────────

    public DbSet<ApplicationUser>    DomainUsers          => Set<ApplicationUser>();
    public DbSet<Event>              Events               => Set<Event>();
    public DbSet<EventTool>          EventTools           => Set<EventTool>();
    public DbSet<ContactBookEntry>   ContactBookEntries   => Set<ContactBookEntry>();
    public DbSet<EventCoordinator>   EventCoordinators    => Set<EventCoordinator>();
    public DbSet<Subscription>          Subscriptions           => Set<Subscription>();
    public DbSet<SubscriptionRequest>   SubscriptionRequests    => Set<SubscriptionRequest>();

    // ── PhotoFrame Module ─────────────────────────────────────────────────────
    public DbSet<PhotoFrameConfig>   PhotoFrameConfigs    => Set<PhotoFrameConfig>();
    public DbSet<LandingPageConfig>  LandingPageConfigs   => Set<LandingPageConfig>();
    public DbSet<PhotoFrameSession>  PhotoFrameSessions   => Set<PhotoFrameSession>();
    public DbSet<CertificateTemplateConfig> CertificateTemplateConfigs => Set<CertificateTemplateConfig>();
    public DbSet<CertificateFieldMapping> CertificateFieldMappings => Set<CertificateFieldMapping>();
    public DbSet<CertificateGenerationBatch> CertificateGenerationBatches => Set<CertificateGenerationBatch>();
    public DbSet<CertificateGenerationItem> CertificateGenerationItems => Set<CertificateGenerationItem>();

    // ── Model Building ────────────────────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Auto-discover all IEntityTypeConfiguration<T> classes in this assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Rename Identity tables to snake_case to be consistent with domain tables
        builder.Entity<AppIdentityUser>().ToTable("identity_users");
        builder.Entity<AppIdentityRole>().ToTable("identity_roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>().ToTable("identity_user_roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("identity_user_claims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("identity_user_logins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("identity_user_tokens");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>().ToTable("identity_role_claims");
    }

    // ── IUnitOfWork ───────────────────────────────────────────────────────────

    /// <summary>
    /// Overrides <c>SaveChangesAsync</c> to automatically stamp audit columns
    /// on all <see cref="AuditableEntity"/> instances before persisting.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        StampAuditColumns();
        return await base.SaveChangesAsync(ct);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private void StampAuditColumns()
    {
        var utcNow = DateTime.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.SetUpdatedAt(utcNow);
        }

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added when userId.HasValue:
                    entry.Entity.SetCreatedBy(userId.Value);
                    break;

                case EntityState.Modified when userId.HasValue:
                    entry.Entity.SetUpdatedBy(userId.Value, utcNow);
                    break;
            }
        }
    }
}
