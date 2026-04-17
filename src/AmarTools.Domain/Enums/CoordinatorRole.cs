namespace AmarTools.Domain.Enums;

/// <summary>
/// Defines the level of access a coordinator has within a specific event.
/// </summary>
public enum CoordinatorRole
{
    /// <summary>Read-only access to the event and its tools.</summary>
    Viewer  = 1,

    /// <summary>Can operate tools but cannot change event settings.</summary>
    Operator = 2,

    /// <summary>Full access equivalent to the event owner; cannot transfer ownership.</summary>
    Manager  = 3
}
