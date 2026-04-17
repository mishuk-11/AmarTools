namespace AmarTools.Modules.PhotoFrame.Contracts;

/// <summary>
/// Returned after a guest's photo has been merged with the frame.
/// The front-end uses <see cref="SessionId"/> for a subsequent download request,
/// <see cref="MergedPhotoUrl"/> for an instant preview, and <see cref="DownloadUrl"/>
/// for the tracked final download action.
/// </summary>
public sealed record ProcessedPhotoDto(
    Guid   SessionId,
    string MergedPhotoUrl,
    string DownloadUrl
);
