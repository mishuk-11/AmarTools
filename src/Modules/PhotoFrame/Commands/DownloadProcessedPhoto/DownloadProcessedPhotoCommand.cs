using AmarTools.BuildingBlocks.Common;
using MediatR;

namespace AmarTools.Modules.PhotoFrame.Commands.DownloadProcessedPhoto;

/// <summary>
/// Public command that marks a processed guest photo as downloaded
/// and returns the public URL for the merged image.
/// </summary>
public sealed record DownloadProcessedPhotoCommand(Guid SessionId)
    : IRequest<Result<string>>;
