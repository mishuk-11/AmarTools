using AmarTools.Domain.Enums;

namespace AmarTools.Modules.Dashboard.Contracts;

/// <summary>
/// Lightweight event summary used in list and dashboard views.
/// Never exposes domain internals directly — the controller returns this.
/// </summary>
public sealed record EventSummaryDto(
    Guid         Id,
    string       Name,
    string?      Description,
    string?      Venue,
    DateOnly?    EventDate,
    EventStatus  Status,
    DateTime     CreatedAt,
    IReadOnlyList<ActiveToolDto> ActiveTools
);
