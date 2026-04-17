using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;

namespace AmarTools.Modules.PhotoFrame.Commands.UploadLogoImage;

/// <summary>
/// Uploads or replaces the logo image shown on the photo frame landing page.
/// </summary>
public sealed record UploadLogoImageCommand(
    Guid PhotoFrameConfigId,
    Stream ImageStream,
    string FileName,
    string ContentType
) : IRequest<Result<PhotoFrameSetupDto>>;
