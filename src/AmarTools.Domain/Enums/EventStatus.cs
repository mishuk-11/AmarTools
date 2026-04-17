namespace AmarTools.Domain.Enums;

/// <summary>
/// Lifecycle states of an <see cref="AmarTools.Domain.Entities.Event"/>.
/// </summary>
public enum EventStatus
{
    /// <summary>The event is active and counts against the 3-event plan limit.</summary>
    Active   = 1,

    /// <summary>
    /// Archived events are retained for reference but do NOT count against the limit.
    /// Tools within an archived event become read-only.
    /// </summary>
    Archived = 2,

    /// <summary>Permanently closed. Tools and data are retained but immutable.</summary>
    Closed   = 3
}
