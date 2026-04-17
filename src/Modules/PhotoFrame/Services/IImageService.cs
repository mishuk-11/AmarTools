namespace AmarTools.Modules.PhotoFrame.Services;

/// <summary>
/// Handles image processing operations for the EventPhotoframeGenerator.
/// Implemented using SixLabors.ImageSharp.
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Composites the guest photo behind the frame overlay to produce the final merged image.
    ///
    /// Layering order (bottom to top):
    /// <list type="number">
    ///   <item>Guest photo — scaled and offset to the user's chosen position.</item>
    ///   <item>Frame PNG — rendered on top; transparent cut-outs reveal the guest photo beneath.</item>
    /// </list>
    /// </summary>
    /// <param name="frameStream">The event's frame PNG (with transparency). Not disposed by this method.</param>
    /// <param name="guestPhotoStream">The guest's uploaded JPG/PNG. Not disposed by this method.</param>
    /// <param name="offsetX">Horizontal offset of the guest photo in pixels (relative to canvas origin).</param>
    /// <param name="offsetY">Vertical offset of the guest photo in pixels (relative to canvas origin).</param>
    /// <param name="scale">Uniform scale factor applied to the guest photo (1.0 = natural size).</param>
    /// <returns>
    /// A PNG-encoded <see cref="Stream"/> of the merged image at the frame's original dimensions.
    /// The caller is responsible for disposing this stream.
    /// </returns>
    Task<Stream> MergeAsync(
        Stream frameStream,
        Stream guestPhotoStream,
        double offsetX,
        double offsetY,
        double scale,
        CancellationToken ct = default);

    /// <summary>
    /// Validates that the uploaded file is a supported image (JPG or PNG)
    /// and does not exceed the size limit.
    /// </summary>
    /// <param name="stream">The uploaded file stream.</param>
    /// <param name="contentType">The MIME type declared by the client.</param>
    /// <param name="maxBytes">Maximum allowed file size in bytes (default 10 MB).</param>
    bool IsValidUpload(Stream stream, string contentType, long maxBytes = 10 * 1024 * 1024);
}
