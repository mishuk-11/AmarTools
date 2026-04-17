using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(300);

        builder.Property(e => e.Description)
               .HasMaxLength(1000);

        builder.Property(e => e.Venue)
               .HasMaxLength(500);

        builder.Property(e => e.Status)
               .IsRequired()
               .HasConversion<int>(); // stored as integer (0=Active, 1=Archived)

        builder.Property(e => e.EventDate);

        // Audit columns
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.CreatedById);
        builder.Property(e => e.UpdatedById);

        // Index for the most common query: get all active events for a user
        builder.HasIndex(e => new { e.OwnerId, e.Status })
               .HasDatabaseName("ix_events_ownerid_status");

        // Tools activated inside this event
        builder.HasMany(e => e.Tools)
               .WithOne(t => t.Event)
               .HasForeignKey(t => t.EventId)
               .OnDelete(DeleteBehavior.Cascade);

        // Coordinator assignments
        builder.HasMany(e => e.Coordinators)
               .WithOne(c => c.Event)
               .HasForeignKey(c => c.EventId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
