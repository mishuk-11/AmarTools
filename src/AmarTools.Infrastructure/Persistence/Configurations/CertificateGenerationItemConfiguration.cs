using AmarTools.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class CertificateGenerationItemConfiguration : IEntityTypeConfiguration<CertificateGenerationItem>
{
    public void Configure(EntityTypeBuilder<CertificateGenerationItem> builder)
    {
        builder.ToTable("certificate_generation_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.SequenceNumber).IsRequired();
        builder.Property(i => i.RecipientName).IsRequired().HasMaxLength(300);
        builder.Property(i => i.RecipientEmail).HasMaxLength(320);
        builder.Property(i => i.PayloadJson).IsRequired();
        builder.Property(i => i.Status).IsRequired().HasMaxLength(50);
        builder.Property(i => i.GeneratedFilePath).HasMaxLength(1000);

        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.UpdatedAt);
        builder.Property(i => i.CreatedById);
        builder.Property(i => i.UpdatedById);

        builder.HasIndex(i => new { i.CertificateGenerationBatchId, i.SequenceNumber })
            .HasDatabaseName("ix_certificate_generation_items_batchid_sequence");
    }
}
