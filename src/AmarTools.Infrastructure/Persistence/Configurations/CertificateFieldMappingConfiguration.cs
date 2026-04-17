using AmarTools.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class CertificateFieldMappingConfiguration : IEntityTypeConfiguration<CertificateFieldMapping>
{
    public void Configure(EntityTypeBuilder<CertificateFieldMapping> builder)
    {
        builder.ToTable("certificate_field_mappings");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.FieldKey).IsRequired().HasMaxLength(100);
        builder.Property(m => m.SourceColumn).IsRequired().HasMaxLength(200);
        builder.Property(m => m.FieldType).IsRequired().HasMaxLength(50);
        builder.Property(m => m.PositionX).IsRequired();
        builder.Property(m => m.PositionY).IsRequired();
        builder.Property(m => m.Width);
        builder.Property(m => m.Height);
        builder.Property(m => m.FontSize);
        builder.Property(m => m.FontColor).HasMaxLength(50);

        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt);
        builder.Property(m => m.CreatedById);
        builder.Property(m => m.UpdatedById);

        builder.HasIndex(m => new { m.CertificateTemplateConfigId, m.FieldKey })
            .HasDatabaseName("ix_certificate_field_mappings_configid_fieldkey");
    }
}
