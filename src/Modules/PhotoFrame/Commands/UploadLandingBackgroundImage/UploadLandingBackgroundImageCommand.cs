using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;

namespace AmarTools.Modules.PhotoFrame.Commands.UploadLandingBackgroundImage;

/// <summary>
/// Uploads or replaces the optional custom landing page background image.
/// </summary>
public sealed record UploadLandingBackgroundImageCommand(
    Guid PhotoFrameConfigId,
    Stream ImageStream,
    string FileName,
    string ContentType
) : IRequest<Result<PhotoFrameSetupDto>>;
