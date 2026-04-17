using AmarTools.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class LandingPageConfigConfiguration : IEntityTypeConfiguration<LandingPageConfig>
{
    public void Configure(EntityTypeBuilder<LandingPageConfig> builder)
    {
        builder.ToTable("landing_page_configs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.TemplateName).IsRequired().HasMaxLength(50);
        builder.Property(l => l.BackgroundColor).IsRequired().HasMaxLength(20);
        builder.Property(l => l.BackgroundImagePath).HasMaxLength(1000);
        builder.Property(l => l.HeadlineText).HasMaxLength(300);
        builder.Property(l => l.InstructionText).IsRequired().HasMaxLength(1000);
        builder.Property(l => l.DownloadButtonText).IsRequired().HasMaxLength(100);

        // Audit columns
        builder.Property(l => l.CreatedAt).IsRequired();
        builder.Property(l => l.UpdatedAt);
        builder.Property(l => l.CreatedById);
        builder.Property(l => l.UpdatedById);
    }
}
