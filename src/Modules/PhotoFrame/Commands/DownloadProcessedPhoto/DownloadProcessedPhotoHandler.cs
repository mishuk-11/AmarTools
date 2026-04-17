using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.PhotoFrame.Commands.DownloadProcessedPhoto;

internal sealed class DownloadProcessedPhotoHandler
    : IRequestHandler<DownloadProcessedPhotoCommand, Result<string>>
{
    private readonly AppDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;

    public DownloadProcessedPhotoHandler(
        AppDbContext db,
        IUnitOfWork uow,
        IFileStorageService storage)
    {
        _db = db;
        _uow = uow;
        _storage = storage;
    }

    public async Task<Result<string>> Handle(
        DownloadProcessedPhotoCommand command,
        CancellationToken ct)
    {
        var session = await _db.PhotoFrameSessions
            .Include(s => s.PhotoFrameConfig)
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, ct);

        if (session is null)
            return Error.NotFound("PhotoFrame.SessionNotFound", "Processed photo session not found.");

        if (!session.PhotoFrameConfig.IsPublished)
            return Error.NotFound("PhotoFrame.NotFound", "This photo frame event could not be found.");

        if (string.IsNullOrWhiteSpace(session.MergedPhotoPath))
            return Error.Failure("PhotoFrame.NotProcessed", "This photo is not ready to download yet.");

        session.RecordDownload();
        await _uow.SaveChangesAsync(ct);

        return _storage.GetPublicUrl(session.MergedPhotoPath);
    }
}
