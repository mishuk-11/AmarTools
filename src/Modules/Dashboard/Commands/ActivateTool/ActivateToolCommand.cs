using AmarTools.BuildingBlocks.Common;
using AmarTools.Domain.Enums;
using MediatR;

namespace AmarTools.Modules.Dashboard.Commands.ActivateTool;

/// <summary>
/// Activates a tool inside an event workspace.
/// If the tool is already active, returns the existing EventTool Id (idempotent).
/// </summary>
/// <param name="EventId">The event to activate the tool on.</param>
/// <param name="ToolType">Which tool to activate.</param>
public sealed record ActivateToolCommand(Guid EventId, ToolType ToolType)
    : IRequest<Result<Guid>>;
