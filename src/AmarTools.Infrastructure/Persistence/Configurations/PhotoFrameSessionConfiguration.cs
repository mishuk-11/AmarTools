using AmarTools.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class PhotoFrameSessionConfiguration : IEntityTypeConfiguration<PhotoFrameSession>
{
    public void Configure(EntityTypeBuilder<PhotoFrameSession> builder)
    {
        builder.ToTable("photo_frame_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.GuestPhotoPath).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.MergedPhotoPath).HasMaxLength(1000);
        builder.Property(s => s.OffsetX).IsRequired();
        builder.Property(s => s.OffsetY).IsRequired();
        builder.Property(s => s.Scale).IsRequired().HasDefaultValue(1.0);
        builder.Property(s => s.DownloadedAt);
        builder.Property(s => s.CreatedAt).IsRequired();

        // Index for querying sessions by frame config (e.g. analytics)
        builder.HasIndex(s => s.PhotoFrameConfigId)
               .HasDatabaseName("ix_photo_frame_sessions_configid");
    }
}
