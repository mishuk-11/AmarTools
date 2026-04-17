using AmarTools.Domain.Enums;

namespace AmarTools.Modules.Dashboard.Contracts;

/// <summary>
/// Full payload returned by the GET /dashboard endpoint.
/// Gives the front-end everything it needs to render the main workspace view.
/// </summary>
public sealed record DashboardDto(
    /// <summary>Active events (counts against the 3-slot limit).</summary>
    IReadOnlyList<EventSummaryDto> ActiveEvents,

    /// <summary>Archived events (not counting against the limit, shown for reference).</summary>
    IReadOnlyList<EventSummaryDto> ArchivedEvents,

    /// <summary>How many active-event slots remain (max 3).</summary>
    int RemainingEventSlots,

    /// <summary>Tools the user has purchased a subscription for.</summary>
    IReadOnlyList<SubscribedToolDto> SubscribedTools
);
