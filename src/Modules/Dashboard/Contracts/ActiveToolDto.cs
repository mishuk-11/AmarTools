using AmarTools.Domain.Enums;

namespace AmarTools.Modules.Dashboard.Contracts;

/// <summary>A tool that has been activated inside an event.</summary>
public sealed record ActiveToolDto(
    Guid     Id,
    ToolType ToolType,
    string   ToolName,
    bool     IsEnabled,
    DateTime ActivatedAt
);
