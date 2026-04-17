using AmarTools.BuildingBlocks.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AmarTools.Infrastructure.Services;

/// <summary>
/// Development file storage — saves files under <c>wwwroot/uploads/{folder}/</c>
/// and serves them as static files via the Web project's static file middleware.
///
/// Replace with an S3 / Azure Blob implementation for production by
/// swapping the DI registration in <c>DependencyInjection.cs</c>.
/// </summary>
internal sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseDirectory;
    private readonly string _baseUrl;

    public LocalFileStorageService(
        IWebHostEnvironment env,
        IConfiguration config)
    {
        // Root: wwwroot/uploads/
        _baseDirectory = Path.Combine(env.WebRootPath, "uploads");
        _baseUrl       = config["App:BaseUrl"]?.TrimEnd('/') ?? "https://localhost:7000";
    }

    /// <inheritdoc />
    public async Task<string> SaveAsync(
        Stream content, string fileName, string folder, CancellationToken ct = default)
    {
        var folderPath = Path.Combine(_baseDirectory, folder);
        Directory.CreateDirectory(folderPath);

        // Ensure unique file name to prevent overwrites
        var safeFileName = $"{Guid.NewGuid():N}_{SanitizeFileName(fileName)}";
        var fullPath     = Path.Combine(folderPath, safeFileName);

        await using var fileStream = new FileStream(
            fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

        await content.CopyToAsync(fileStream, ct);

        // Return storage-relative path (what goes in the DB)
        return Path.Combine(folder, safeFileName).Replace('\\', '/');
    }

    /// <inheritdoc />
    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_baseDirectory, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_baseDirectory, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Stored file could not be found.", fullPath);

        Stream stream = new FileStream(
            fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);

        return Task.FromResult(stream);
    }

    /// <inheritdoc />
    public string GetPublicUrl(string storagePath)
        => $"/uploads/{storagePath.TrimStart('/')}";

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(c => invalid.Contains(c) ? '_' : c));
    }
}
