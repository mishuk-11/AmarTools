using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.PhotoFrame.Contracts;
using AmarTools.Modules.PhotoFrame.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.PhotoFrame.Commands.UploadLandingBackgroundImage;

internal sealed class UploadLandingBackgroundImageHandler
    : IRequestHandler<UploadLandingBackgroundImageCommand, Result<PhotoFrameSetupDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;
    private readonly IImageService _imageService;

    public UploadLandingBackgroundImageHandler(
        AppDbContext db,
        ICurrentUserService currentUser,
        IUnitOfWork uow,
        IFileStorageService storage,
        IImageService imageService)
    {
        _db = db;
        _currentUser = currentUser;
        _uow = uow;
        _storage = storage;
        _imageService = imageService;
    }

    public async Task<Result<PhotoFrameSetupDto>> Handle(
        UploadLandingBackgroundImageCommand command,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        if (!_imageService.IsValidUpload(command.ImageStream, command.ContentType))
            return Error.Validation("PhotoFrame.InvalidFile",
                "Only JPG and PNG files up to 10 MB are accepted as background images.");

        var config = await _db.PhotoFrameConfigs
            .Include(c => c.EventTool).ThenInclude(t => t.Event)
            .Include(c => c.LandingPage)
            .FirstOrDefaultAsync(c => c.Id == command.PhotoFrameConfigId, ct);

        if (config is null)
            return Error.NotFound("PhotoFrame.NotFound", "Photo frame config not found.");

        if (config.EventTool.Event.OwnerId != userId)
            return Error.Forbidden("PhotoFrame.Forbidden", "You do not own this frame config.");

        var landingPage = config.LandingPage;
        if (landingPage is null)
        {
            landingPage = LandingPageConfig.CreateDefault(config.Id);
            _db.LandingPageConfigs.Add(landingPage);
        }

        if (!string.IsNullOrWhiteSpace(landingPage.BackgroundImagePath))
            await _storage.DeleteAsync(landingPage.BackgroundImagePath, ct);

        command.ImageStream.Position = 0;
        var storagePath = await _storage.SaveAsync(
            command.ImageStream, command.FileName, "landing-backgrounds", ct);

        landingPage.Update(
            landingPage.TemplateName,
            landingPage.BackgroundColor,
            storagePath,
            landingPage.HeadlineText,
            landingPage.InstructionText,
            landingPage.DownloadButtonText);

        await _uow.SaveChangesAsync(ct);

        await _db.Entry(config).Reference(c => c.LandingPage).LoadAsync(ct);
        return config.ToSetupDto(_storage);
    }
}
