using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.PhotoFrame.Contracts;
using AmarTools.Modules.PhotoFrame.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.PhotoFrame.Commands.ProcessGuestPhoto;

/// <summary>
/// Public handler — no authentication check.
/// Validates the slug, processes the image, saves the session, returns the merged URL.
/// </summary>
internal sealed class ProcessGuestPhotoHandler
    : IRequestHandler<ProcessGuestPhotoCommand, Result<ProcessedPhotoDto>>
{
    private readonly AppDbContext        _db;
    private readonly IUnitOfWork         _uow;
    private readonly IFileStorageService _storage;
    private readonly IImageService       _imageService;

    public ProcessGuestPhotoHandler(
        AppDbContext db, IUnitOfWork uow,
        IFileStorageService storage, IImageService imageService)
    {
        _db           = db;
        _uow          = uow;
        _storage      = storage;
        _imageService = imageService;
    }

    public async Task<Result<ProcessedPhotoDto>> Handle(
        ProcessGuestPhotoCommand command, CancellationToken ct)
    {
        // ── 1. Resolve frame config by public slug ────────────────────────────
        var config = await _db.PhotoFrameConfigs
            .FirstOrDefaultAsync(c => c.SharingSlug == command.SharingSlug, ct);

        if (config is null || !config.IsPublished)
            return Error.NotFound("PhotoFrame.NotFound",
                "This photo frame event could not be found or is not currently active.");

        if (string.IsNullOrWhiteSpace(config.FrameImagePath))
            return Error.Failure("PhotoFrame.NoFrame",
                "The frame image for this event has not been configured yet.");

        // ── 2. Validate guest upload ──────────────────────────────────────────
        if (!_imageService.IsValidUpload(command.GuestPhotoStream, command.ContentType))
            return Error.Validation("PhotoFrame.InvalidUpload",
                "Please upload a JPG or PNG image under 10 MB.");

        // ── 3. Save raw guest photo ───────────────────────────────────────────
        command.GuestPhotoStream.Position = 0;
        var guestPhotoPath = await _storage.SaveAsync(
            command.GuestPhotoStream, command.FileName, "guest-photos", ct);

        // ── 4. Load frame from storage and merge ──────────────────────────────
        var frameFullPath = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot", "uploads",
            config.FrameImagePath.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(frameFullPath))
            return Error.Failure("PhotoFrame.FrameFileMissing",
                "The configured frame image file could not be found on the server.");

        await using var frameStream       = File.OpenRead(frameFullPath);
        command.GuestPhotoStream.Position = 0;

        await using var mergedStream = await _imageService.MergeAsync(
            frameStream,
            command.GuestPhotoStream,
            command.OffsetX,
            command.OffsetY,
            command.Scale,
            ct);

        // ── 5. Save merged output ─────────────────────────────────────────────
        var mergedFileName  = $"merged_{Guid.NewGuid():N}.png";
        var mergedPhotoPath = await _storage.SaveAsync(
            mergedStream, mergedFileName, "merged-photos", ct);

        // ── 6. Persist session record ─────────────────────────────────────────
        var session = PhotoFrameSession.Create(
            config.Id, guestPhotoPath,
            command.OffsetX, command.OffsetY, command.Scale);

        session.SetMergedPhoto(mergedPhotoPath);
        _db.PhotoFrameSessions.Add(session);
        await _uow.SaveChangesAsync(ct);

        return new ProcessedPhotoDto(
            session.Id,
            MergedPhotoUrl: _storage.GetPublicUrl(mergedPhotoPath),
            DownloadUrl: $"/api/photo-frame/public/download/{session.Id}");
    }
}
