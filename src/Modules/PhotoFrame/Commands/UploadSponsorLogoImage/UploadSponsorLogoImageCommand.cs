using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;

namespace AmarTools.Modules.PhotoFrame.Commands.UploadSponsorLogoImage;

/// <summary>
/// Uploads or replaces the sponsor logo image for a photo frame configuration.
/// </summary>
public sealed record UploadSponsorLogoImageCommand(
    Guid PhotoFrameConfigId,
    Stream ImageStream,
    string FileName,
    string ContentType
) : IRequest<Result<PhotoFrameSetupDto>>;
