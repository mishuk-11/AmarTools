using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;

namespace AmarTools.Modules.PhotoFrame.Commands.UpdateLandingPage;

/// <summary>
/// Creates or updates the landing page design for a photo frame config.
/// Also controls <see cref="Publish"/> — setting it to <c>true</c> makes
/// the guest landing page publicly accessible.
/// </summary>
public sealed record UpdateLandingPageCommand(
    Guid    PhotoFrameConfigId,
    string  TemplateName,
    string  BackgroundColor,
    string? HeadlineText,
    string? InstructionText,
    string? DownloadButtonText,
    bool    Publish
) : IRequest<Result<PhotoFrameSetupDto>>;
