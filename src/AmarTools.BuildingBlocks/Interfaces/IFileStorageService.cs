namespace AmarTools.BuildingBlocks.Interfaces;

/// <summary>
/// Abstracts file persistence so modules never depend on a specific storage
/// backend (local disk, S3, Azure Blob, etc.).
///
/// Implementations:
/// <list type="bullet">
///   <item><b>Dev:</b> <c>LocalFileStorageService</c> in Infrastructure — saves under wwwroot/uploads.</item>
///   <item><b>Prod:</b> Swap to an S3/Azure Blob implementation without touching module code.</item>
/// </list>
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Persists a file and returns a storage-relative path (e.g. <c>frames/abc123.png</c>).
    /// The returned path is what you store in the database.
    /// </summary>
    /// <param name="content">File byte stream (not closed by this method).</param>
    /// <param name="fileName">Original or generated file name including extension.</param>
    /// <param name="folder">Logical sub-folder (e.g. <c>"frames"</c>, <c>"guest-photos"</c>).</param>
    Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken ct = default);

    /// <summary>Deletes a previously stored file by its storage-relative path.</summary>
    Task DeleteAsync(string storagePath, CancellationToken ct = default);

    /// <summary>
    /// Opens a previously stored file for reading.
    /// The caller is responsible for disposing the returned stream.
    /// </summary>
    Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct = default);

    /// <summary>
    /// Returns the full public URL for a stored file.
    /// For local storage: <c>https://host/uploads/{storagePath}</c>.
    /// For cloud storage: a pre-signed or public blob URL.
    /// </summary>
    string GetPublicUrl(string storagePath);
}
