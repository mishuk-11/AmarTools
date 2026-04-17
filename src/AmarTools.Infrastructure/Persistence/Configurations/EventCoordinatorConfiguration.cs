using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class EventCoordinatorConfiguration : IEntityTypeConfiguration<EventCoordinator>
{
    public void Configure(EntityTypeBuilder<EventCoordinator> builder)
    {
        builder.ToTable("event_coordinators");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Role)
               .IsRequired()
               .HasConversion<int>();

        builder.Property(c => c.GrantedPermissions)
               .HasMaxLength(2000)
               .HasDefaultValue(string.Empty);

        builder.Property(c => c.IsActive)
               .HasDefaultValue(true);

        // Audit columns
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt);
        builder.Property(c => c.CreatedById);
        builder.Property(c => c.UpdatedById);

        // One coordinator per user per event (use IsActive to soft-revoke)
        builder.HasIndex(c => new { c.EventId, c.CoordinatorUserId })
               .IsUnique()
               .HasDatabaseName("ix_coordinators_eventid_userid");

        // FK: coordinator user (restrict delete — coordinator must be explicitly removed first)
        builder.HasOne(c => c.CoordinatorUser)
               .WithMany(u => u.CoordinatorAssignments)
               .HasForeignKey(c => c.CoordinatorUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
