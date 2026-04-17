using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;

namespace AmarTools.Modules.PhotoFrame.Commands.UploadFrameImage;

/// <summary>
/// Uploads the blank PNG frame image for a photo frame config.
/// Replaces any previously uploaded frame.
/// </summary>
public sealed record UploadFrameImageCommand(
    Guid   PhotoFrameConfigId,
    Stream ImageStream,
    string FileName,
    string ContentType
) : IRequest<Result<PhotoFrameSetupDto>>;
