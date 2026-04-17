using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AmarTools.Modules.PhotoFrame.Services;

/// <summary>
/// SixLabors.ImageSharp implementation of <see cref="IImageService"/>.
///
/// Merge algorithm:
/// 1. Load the frame PNG to determine canvas dimensions (W × H).
/// 2. Load the guest photo; apply scale then calculate its draw position.
/// 3. Create a new RGBA32 canvas at frame dimensions, filled transparent.
/// 4. Draw the (scaled + offset) guest photo onto the canvas.
/// 5. Draw the frame PNG on top — its opaque pixels cover the guest photo,
///    its transparent pixels let the guest photo show through.
/// 6. Encode to PNG and return.
/// </summary>
internal sealed class ImageSharpService : IImageService
{
    private static readonly HashSet<string> AllowedMimeTypes =
    [
        "image/jpeg", "image/jpg", "image/png"
    ];

    /// <inheritdoc />
    public async Task<Stream> MergeAsync(
        Stream frameStream,
        Stream guestPhotoStream,
        double offsetX,
        double offsetY,
        double scale,
        CancellationToken ct = default)
    {
        using var frame      = await Image.LoadAsync<Rgba32>(frameStream, ct);
        using var guestPhoto = await Image.LoadAsync<Rgba32>(guestPhotoStream, ct);

        var canvasW = frame.Width;
        var canvasH = frame.Height;

        // ── Scale guest photo ─────────────────────────────────────────────────
        var scaledW = (int)Math.Round(guestPhoto.Width  * scale);
        var scaledH = (int)Math.Round(guestPhoto.Height * scale);

        // Clamp to reasonable bounds to prevent memory abuse
        scaledW = Math.Clamp(scaledW, 1, canvasW * 4);
        scaledH = Math.Clamp(scaledH, 1, canvasH * 4);

        guestPhoto.Mutate(ctx => ctx.Resize(scaledW, scaledH));

        // ── Build composite ───────────────────────────────────────────────────
        using var canvas = new Image<Rgba32>(canvasW, canvasH);

        canvas.Mutate(ctx =>
        {
            // Layer 1: guest photo at offset position
            ctx.DrawImage(
                guestPhoto,
                new Point((int)Math.Round(offsetX), (int)Math.Round(offsetY)),
                1f);

            // Layer 2: frame on top (transparent areas let guest photo show through)
            ctx.DrawImage(frame, new Point(0, 0), 1f);
        });

        // ── Encode and return ─────────────────────────────────────────────────
        var outputStream = new MemoryStream();
        await canvas.SaveAsync(outputStream, new PngEncoder(), ct);
        outputStream.Position = 0;
        return outputStream;
    }

    /// <inheritdoc />
    public bool IsValidUpload(Stream stream, string contentType, long maxBytes = 10 * 1024 * 1024)
    {
        if (!AllowedMimeTypes.Contains(contentType.ToLowerInvariant()))
            return false;

        if (stream.Length > maxBytes)
            return false;

        return true;
    }
}
