using AmarTools.BuildingBlocks.Domain;
using AmarTools.Domain.Enums;

namespace AmarTools.Domain.Entities;

/// <summary>
/// Junction entity representing a <see cref="ToolType"/> that has been
/// activated (imported) inside a specific <see cref="Event"/>.
///
/// One row per (Event × Tool) pair. The module-specific configuration
/// (e.g., PhotoFrame settings, Cheque templates) is stored in module-owned
/// tables that reference this entity's <see cref="BaseEntity.Id"/>.
/// </summary>
public sealed class EventTool : BaseEntity
{
    /// <summary>FK to the parent event.</summary>
    public Guid EventId { get; private set; }

    /// <summary>Navigation property to the parent event.</summary>
    public Event Event { get; private set; } = null!;

    /// <summary>Which tool has been activated.</summary>
    public ToolType ToolType { get; private set; }

    /// <summary>UTC timestamp when the tool was activated inside this event.</summary>
    public DateTime ActivatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>Whether the tool is currently enabled. Owners can disable without removing.</summary>
    public bool IsEnabled { get; private set; } = true;

    // ── Factory ───────────────────────────────────────────────────────────────

    private EventTool() { } // EF Core

    public static EventTool Create(Guid eventId, ToolType toolType) => new()
    {
        EventId      = eventId,
        ToolType     = toolType,
        ActivatedAt  = DateTime.UtcNow,
        IsEnabled    = true
    };

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>Disables the tool within this event without removing it.</summary>
    public void Disable() => IsEnabled = false;

    /// <summary>Re-enables a previously disabled tool.</summary>
    public void Enable() => IsEnabled = true;
}
