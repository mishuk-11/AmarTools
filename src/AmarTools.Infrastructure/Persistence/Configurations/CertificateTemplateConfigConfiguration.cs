using AmarTools.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class CertificateTemplateConfigConfiguration : IEntityTypeConfiguration<CertificateTemplateConfig>
{
    public void Configure(EntityTypeBuilder<CertificateTemplateConfig> builder)
    {
        builder.ToTable("certificate_template_configs");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TemplateName).IsRequired().HasMaxLength(300);
        builder.Property(c => c.BaseTemplatePath).HasMaxLength(1000);
        builder.Property(c => c.BaseTemplateFileName).HasMaxLength(255);
        builder.Property(c => c.BaseTemplateFileType).HasMaxLength(50);
        builder.Property(c => c.RecipientDatasetPath).HasMaxLength(1000);
        builder.Property(c => c.RecipientDatasetFileName).HasMaxLength(255);
        builder.Property(c => c.EmailSubject).HasMaxLength(300);
        builder.Property(c => c.EmailBody).HasMaxLength(4000);

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt);
        builder.Property(c => c.CreatedById);
        builder.Property(c => c.UpdatedById);

        builder.HasIndex(c => c.EventToolId)
            .IsUnique()
            .HasDatabaseName("ix_certificate_template_configs_eventtoolid");

        builder.HasOne(c => c.EventTool)
            .WithMany()
            .HasForeignKey(c => c.EventToolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.FieldMappings)
            .WithOne(m => m.CertificateTemplateConfig)
            .HasForeignKey(m => m.CertificateTemplateConfigId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
