using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;

namespace AmarTools.Modules.PhotoFrame.Queries.GetPhotoFrameSetup;

/// <summary>Returns the admin setup view for a photo frame config by its EventTool Id.</summary>
public sealed record GetPhotoFrameSetupQuery(Guid EventToolId)
    : IRequest<Result<PhotoFrameSetupDto>>;
