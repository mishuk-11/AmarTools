using AmarTools.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class CertificateGenerationBatchConfiguration : IEntityTypeConfiguration<CertificateGenerationBatch>
{
    public void Configure(EntityTypeBuilder<CertificateGenerationBatch> builder)
    {
        builder.ToTable("certificate_generation_batches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Status).IsRequired().HasMaxLength(50);
        builder.Property(b => b.OutputFormat).IsRequired().HasMaxLength(20);
        builder.Property(b => b.TotalRecipients).IsRequired();
        builder.Property(b => b.CompletedRecipients).IsRequired();
        builder.Property(b => b.FailedRecipients).IsRequired();

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt);
        builder.Property(b => b.CreatedById);
        builder.Property(b => b.UpdatedById);

        builder.HasOne(b => b.CertificateTemplateConfig)
            .WithMany()
            .HasForeignKey(b => b.CertificateTemplateConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Items)
            .WithOne(i => i.CertificateGenerationBatch)
            .HasForeignKey(i => i.CertificateGenerationBatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
