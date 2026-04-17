using AmarTools.BuildingBlocks.Domain;

namespace AmarTools.Domain.Entities;

/// <summary>
/// Tracks a single guest interaction with the photo frame tool:
/// their uploaded photo, the positioning parameters they applied,
/// and the path to the merged output image.
///
/// Sessions are anonymous (no user account required).
/// Created when a guest uploads a photo; updated when they download.
/// </summary>
public sealed class PhotoFrameSession : BaseEntity
{
    /// <summary>FK to the <see cref="PhotoFrameConfig"/> this session belongs to.</summary>
    public Guid PhotoFrameConfigId { get; private set; }

    /// <summary>Navigation to the parent frame config.</summary>
    public PhotoFrameConfig PhotoFrameConfig { get; private set; } = null!;

    // ── Guest photo positioning ───────────────────────────────────────────────

    /// <summary>Storage-relative path of the raw guest-uploaded photo.</summary>
    public string GuestPhotoPath { get; private set; } = string.Empty;

    /// <summary>
    /// Horizontal offset (px) of the guest photo relative to the frame canvas origin.
    /// Positive = shifted right.
    /// </summary>
    public double OffsetX { get; private set; }

    /// <summary>
    /// Vertical offset (px) of the guest photo relative to the frame canvas origin.
    /// Positive = shifted down.
    /// </summary>
    public double OffsetY { get; private set; }

    /// <summary>
    /// Uniform scale factor applied to the guest photo before compositing.
    /// 1.0 = original size, 1.5 = 50% larger, 0.8 = 20% smaller.
    /// </summary>
    public double Scale { get; private set; } = 1.0;

    // ── Output ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Storage-relative path of the merged (frame + guest photo) output image.
    /// <c>null</c> until <see cref="SetMergedPhoto"/> is called after processing.
    /// </summary>
    public string? MergedPhotoPath { get; private set; }

    /// <summary><c>true</c> once the merged image has been generated and is ready to download.</summary>
    public bool IsProcessed => MergedPhotoPath is not null;

    /// <summary>UTC time the guest downloaded the final image. Null if not yet downloaded.</summary>
    public DateTime? DownloadedAt { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    private PhotoFrameSession() { }

    /// <summary>Opens a new session when a guest uploads their photo.</summary>
    public static PhotoFrameSession Create(
        Guid photoFrameConfigId, string guestPhotoPath,
        double offsetX, double offsetY, double scale)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(guestPhotoPath);

        return new PhotoFrameSession
        {
            PhotoFrameConfigId = photoFrameConfigId,
            GuestPhotoPath     = guestPhotoPath,
            OffsetX            = offsetX,
            OffsetY            = offsetY,
            Scale              = Math.Clamp(scale, 0.1, 10.0)
        };
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>Saves the merged image path once image processing completes.</summary>
    public void SetMergedPhoto(string mergedPhotoPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mergedPhotoPath);
        MergedPhotoPath = mergedPhotoPath;
    }

    /// <summary>Records that the guest downloaded their merged photo.</summary>
    public void RecordDownload() => DownloadedAt = DateTime.UtcNow;
}
