using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AmarTools.Infrastructure.Persistence.Configurations;

internal sealed class EventToolConfiguration : IEntityTypeConfiguration<EventTool>
{
    public void Configure(EntityTypeBuilder<EventTool> builder)
    {
        builder.ToTable("event_tools");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ToolType)
               .IsRequired()
               .HasConversion<int>();

        builder.Property(t => t.ActivatedAt).IsRequired();
        // Do NOT use HasDefaultValue here — it causes EF Core to mark IsEnabled as
        // ValueGeneratedOnAdd, which omits it from INSERT and relies on RETURNING.
        // IsEnabled is always set explicitly in EventTool.Create(), so include it directly.
        builder.Property(t => t.IsEnabled).ValueGeneratedNever();

        // Enforce uniqueness: one tool type per event
        builder.HasIndex(t => new { t.EventId, t.ToolType })
               .IsUnique()
               .HasDatabaseName("ix_event_tools_eventid_tooltype");
    }
}
