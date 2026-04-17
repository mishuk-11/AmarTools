using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionRequestConfiguration
    : IEntityTypeConfiguration<SubscriptionRequest>
{
    public void Configure(EntityTypeBuilder<SubscriptionRequest> builder)
    {
        builder.ToTable("subscription_requests");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.PackageDays).IsRequired();

        builder.Property(r => r.Status)
               .IsRequired()
               .HasConversion<int>();

        builder.Property(r => r.RequestedAt).IsRequired();
        builder.Property(r => r.ReviewedAt);
        builder.Property(r => r.ReviewedById);
        builder.Property(r => r.AdminNotes).HasMaxLength(1000);

        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt);
        builder.Property(r => r.CreatedById);
        builder.Property(r => r.UpdatedById);

        // One user can have many requests (e.g. after rejection they can re-apply)
        builder.HasOne(r => r.User)
               .WithMany()
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.UserId, r.Status })
               .HasDatabaseName("ix_subscription_requests_userid_status");

        builder.HasIndex(r => r.Status)
               .HasDatabaseName("ix_subscription_requests_status");
    }
}
