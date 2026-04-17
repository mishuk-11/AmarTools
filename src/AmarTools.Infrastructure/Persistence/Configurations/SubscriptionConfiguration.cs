using AmarTools.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ToolType)
               .IsRequired()
               .HasConversion<int>();

        builder.Property(s => s.StartedAt).IsRequired();
        builder.Property(s => s.ExpiresAt);

        builder.Property(s => s.IsRevoked)
               .HasDefaultValue(false);

        // Audit columns
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt);
        builder.Property(s => s.CreatedById);
        builder.Property(s => s.UpdatedById);

        // Query: is user subscribed to a specific tool?
        builder.HasIndex(s => new { s.UserId, s.ToolType })
               .HasDatabaseName("ix_subscriptions_userid_tooltype");
    }
}
