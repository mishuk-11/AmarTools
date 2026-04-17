using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;

namespace AmarTools.Modules.Dashboard.Contracts;

/// <summary>
/// Domain-to-DTO mapping helpers for the Dashboard module.
/// Kept as extension methods so handlers stay focused on orchestration logic.
/// </summary>
internal static class MappingExtensions
{
    internal static EventSummaryDto ToSummaryDto(this Event e) =>
        new(
            e.Id,
            e.Name,
            e.Description,
            e.Venue,
            e.EventDate,
            e.Status,
            e.CreatedAt,
            e.Tools
             .Where(t => t.IsEnabled)
             .Select(t => new ActiveToolDto(
                 t.Id,
                 t.ToolType,
                 t.ToolType.ToDisplayName(),
                 t.IsEnabled,
                 t.ActivatedAt))
             .ToList()
        );

    internal static string ToDisplayName(this ToolType toolType) => toolType switch
    {
        ToolType.EventPhotoframeGenerator => "Event Photo Frame",
        ToolType.CertificateGenerator     => "Certificate Generator",
        ToolType.ChequePrinting           => "Cheque Printing",
        ToolType.Voting                   => "Voting",
        _                                 => toolType.ToString()
    };

    // ── Contact Book mappings ─────────────────────────────────────────────────

    /// <summary>Maps a plain (non-platform) contact entry to its DTO.</summary>
    internal static ContactDto ToContactDto(this ContactBookEntry c) =>
        new(
            c.Id,
            IsPlatformUser: false,
            DisplayName:    c.ContactName  ?? string.Empty,
            DisplayEmail:   c.ContactEmail ?? string.Empty,
            c.ContactPhone,
            c.Notes,
            LinkedUserId:   null,
            c.CreatedAt
        );

    /// <summary>Maps a platform-linked contact entry to its DTO using the linked user's live profile.</summary>
    internal static ContactDto ToContactDto(this ContactBookEntry c, ApplicationUser? linkedUser) =>
        new(
            c.Id,
            IsPlatformUser: true,
            DisplayName:    linkedUser?.FullName ?? string.Empty,
            DisplayEmail:   linkedUser?.Email    ?? string.Empty,
            Phone:          null,
            c.Notes,
            c.LinkedUserId,
            c.CreatedAt
        );

    // ── Coordinator mappings ──────────────────────────────────────────────────

    /// <summary>Maps an EventCoordinator to its DTO, resolving the user's display fields.</summary>
    internal static CoordinatorDto ToCoordinatorDto(
        this EventCoordinator ec, ApplicationUser coordinatorUser) =>
        new(
            ec.Id,
            ec.EventId,
            ec.CoordinatorUserId,
            FullName:            coordinatorUser.FullName,
            Email:               coordinatorUser.Email,
            ec.Role,
            GrantedPermissions:  ec.GetPermissions(),
            ec.IsActive,
            AssignedAt:          ec.CreatedAt
        );
}
