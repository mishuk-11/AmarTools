using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;

namespace AmarTools.Modules.PhotoFrame.Commands.ProcessGuestPhoto;

/// <summary>
/// Public (unauthenticated) command: merges a guest's uploaded photo
/// with the event frame and returns the URL to the merged image.
///
/// The <see cref="SharingSlug"/> identifies the frame config without
/// exposing internal IDs on the guest-facing URL.
/// </summary>
public sealed record ProcessGuestPhotoCommand(
    string SharingSlug,
    Stream GuestPhotoStream,
    string FileName,
    string ContentType,
    double OffsetX,
    double OffsetY,
    double Scale
) : IRequest<Result<ProcessedPhotoDto>>;
