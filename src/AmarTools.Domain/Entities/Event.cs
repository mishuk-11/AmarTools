using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Domain;
using AmarTools.Domain.Enums;

namespace AmarTools.Domain.Entities;

/// <summary>
/// The primary workspace/tenant unit in AmarTools.
/// Each Event is an isolated container for tools purchased by its owner.
///
/// Business rules enforced here:
/// <list type="bullet">
///   <item>An owner may have at most <see cref="MaxActiveEvents"/> active events simultaneously.</item>
///   <item>Archiving or closing an event frees up that slot.</item>
///   <item>Tools can only be activated on an <see cref="EventStatus.Active"/> event.</item>
/// </list>
/// </summary>
public sealed class Event : AuditableEntity
{
    /// <summary>Plan limit: maximum number of simultaneously <see cref="EventStatus.Active"/> events per user.</summary>
    public const int MaxActiveEvents = 3;

    // ── Identity ──────────────────────────────────────────────────────────────

    /// <summary>Human-readable event name (e.g. "Annual Tech Summit 2025").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Optional description shown in the dashboard.</summary>
    public string? Description { get; private set; }

    /// <summary>Scheduled start date of the real-world event.</summary>
    public DateOnly? EventDate { get; private set; }

    /// <summary>Physical or virtual venue.</summary>
    public string? Venue { get; private set; }

    /// <summary>Current lifecycle state.</summary>
    public EventStatus Status { get; private set; } = EventStatus.Active;

    // ── Ownership ─────────────────────────────────────────────────────────────

    /// <summary>FK to the <see cref="ApplicationUser"/> who owns this event.</summary>
    public Guid OwnerId { get; private set; }

    /// <summary>Navigation property to the owner.</summary>
    public ApplicationUser Owner { get; private set; } = null!;

    // ── Activated Tools ───────────────────────────────────────────────────────

    private readonly List<EventTool> _tools = [];
    /// <summary>Tools that have been activated (imported) into this event.</summary>
    public IReadOnlyCollection<EventTool> Tools => _tools.AsReadOnly();

    // ── Coordinators ──────────────────────────────────────────────────────────

    private readonly List<EventCoordinator> _coordinators = [];
    /// <summary>Coordinators assigned access to this event.</summary>
    public IReadOnlyCollection<EventCoordinator> Coordinators => _coordinators.AsReadOnly();

    // ── Factory ───────────────────────────────────────────────────────────────

    private Event() { } // EF Core

    /// <summary>
    /// Creates a new active event.
    /// The caller (service layer) must verify the owner has not exceeded
    /// <see cref="MaxActiveEvents"/> before calling this factory.
    /// </summary>
    public static Event Create(string name, Guid ownerId, string? description = null,
        DateOnly? eventDate = null, string? venue = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (ownerId == Guid.Empty) throw new ArgumentException("OwnerId cannot be empty.", nameof(ownerId));

        return new Event
        {
            Name        = name.Trim(),
            OwnerId     = ownerId,
            Description = description?.Trim(),
            EventDate   = eventDate,
            Venue       = venue?.Trim(),
            Status      = EventStatus.Active
        };
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Activates a tool inside this event.
    /// Returns <see cref="Error.Failure"/> if the tool is already active
    /// or if the event is not in <see cref="EventStatus.Active"/> state.
    /// </summary>
    public Result<EventTool> ActivateTool(ToolType toolType)
    {
        if (Status != EventStatus.Active)
            return Error.Failure("Event.NotActive",
                "Tools can only be activated on an active event.");

        if (_tools.Any(t => t.ToolType == toolType))
            return Error.Conflict("Event.ToolAlreadyActive",
                $"The tool '{toolType}' is already active in this event.");

        var tool = EventTool.Create(Id, toolType);
        _tools.Add(tool);
        return tool;
    }

    /// <summary>Archives the event, freeing up the active-event slot.</summary>
    public Result Archive()
    {
        if (Status == EventStatus.Archived)
            return Error.Failure("Event.AlreadyArchived", "This event is already archived.");

        if (Status == EventStatus.Closed)
            return Error.Failure("Event.Closed", "A closed event cannot be archived.");

        Status = EventStatus.Archived;
        return Result.Ok;
    }

    /// <summary>Permanently closes the event.</summary>
    public Result Close()
    {
        if (Status == EventStatus.Closed)
            return Error.Failure("Event.AlreadyClosed", "This event is already closed.");

        Status = EventStatus.Closed;
        return Result.Ok;
    }

    /// <summary>Re-activates an archived event if the owner is within the slot limit.</summary>
    /// <param name="currentActiveCount">
    /// Active event count for this owner — must be provided by the service layer.
    /// </param>
    public Result Reactivate(int currentActiveCount)
    {
        if (Status != EventStatus.Archived)
            return Error.Failure("Event.NotArchived", "Only archived events can be reactivated.");

        if (currentActiveCount >= MaxActiveEvents)
            return Error.Failure("Event.LimitReached",
                $"You can have at most {MaxActiveEvents} active events. Archive another event first.");

        Status = EventStatus.Active;
        return Result.Ok;
    }

    /// <summary>Updates mutable event metadata.</summary>
    public void UpdateDetails(string name, string? description, DateOnly? eventDate, string? venue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name        = name.Trim();
        Description = description?.Trim();
        EventDate   = eventDate;
        Venue       = venue?.Trim();
    }
}
