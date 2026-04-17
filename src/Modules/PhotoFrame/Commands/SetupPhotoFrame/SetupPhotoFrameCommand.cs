using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;

namespace AmarTools.Modules.PhotoFrame.Commands.SetupPhotoFrame;

/// <summary>
/// Creates (or updates) the photo frame configuration for an event tool.
/// Idempotent — safe to call multiple times for the same <see cref="EventToolId"/>.
/// </summary>
public sealed record SetupPhotoFrameCommand(
    Guid      EventToolId,
    string    EventName,
    string?   SponsorName,
    string?   VenueName,
    DateTime? EventDateTime
) : IRequest<Result<PhotoFrameSetupDto>>;
