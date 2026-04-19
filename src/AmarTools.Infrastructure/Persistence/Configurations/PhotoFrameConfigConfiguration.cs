using AmarTools.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class PhotoFrameConfigConfiguration : IEntityTypeConfiguration<PhotoFrameConfig>
{
    public void Configure(EntityTypeBuilder<PhotoFrameConfig> builder)
    {
        builder.ToTable("photo_frame_configs");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.EventName).IsRequired().HasMaxLength(300);
        builder.Property(p => p.SponsorName).HasMaxLength(300);
        builder.Property(p => p.VenueName).HasMaxLength(500);
        builder.Property(p => p.FrameImagePath).HasMaxLength(1000);
        builder.Property(p => p.LogoImagePath).HasMaxLength(1000);
        builder.Property(p => p.SponsorLogoPath).HasMaxLength(1000);
        // Do NOT use HasDefaultValue here — it causes EF Core to mark IsPublished as
        // ValueGeneratedOnAdd and omit it from INSERT statements (relying on RETURNING).
        // We always set IsPublished explicitly in PhotoFrameConfig.Create(), so EF Core
        // should include the value directly in every INSERT.
        builder.Property(p => p.IsPublished).ValueGeneratedNever();

        builder.Property(p => p.SharingSlug)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasIndex(p => p.SharingSlug)
               .IsUnique()
               .HasDatabaseName("ix_photo_frame_configs_slug");

        builder.HasIndex(p => p.EventToolId)
               .IsUnique()                       // One config per EventTool activation
               .HasDatabaseName("ix_photo_frame_configs_eventtoolid");

        // Audit columns
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt);
        builder.Property(p => p.CreatedById);
        builder.Property(p => p.UpdatedById);

        // One-to-one with LandingPageConfig
        builder.HasOne(p => p.LandingPage)
               .WithOne(l => l.PhotoFrameConfig)
               .HasForeignKey<LandingPageConfig>(l => l.PhotoFrameConfigId)
               .OnDelete(DeleteBehavior.Cascade);

        // Sessions (one-to-many)
        builder.HasMany<PhotoFrameSession>()
               .WithOne(s => s.PhotoFrameConfig)
               .HasForeignKey(s => s.PhotoFrameConfigId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
