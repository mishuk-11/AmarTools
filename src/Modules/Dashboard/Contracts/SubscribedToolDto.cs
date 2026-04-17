using AmarTools.Domain.Enums;

namespace AmarTools.Modules.Dashboard.Contracts;

/// <summary>A tool the user holds an active subscription for.</summary>
public sealed record SubscribedToolDto(
    Guid     SubscriptionId,
    ToolType ToolType,
    string   ToolName,
    DateTime StartedAt,
    DateTime? ExpiresAt
);
