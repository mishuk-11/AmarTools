using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.PhotoFrame.Contracts;
using AmarTools.Modules.PhotoFrame.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.PhotoFrame.Commands.UploadSponsorLogoImage;

internal sealed class UploadSponsorLogoImageHandler
    : IRequestHandler<UploadSponsorLogoImageCommand, Result<PhotoFrameSetupDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;
    private readonly IImageService _imageService;

    public UploadSponsorLogoImageHandler(
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
        UploadSponsorLogoImageCommand command,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        if (!_imageService.IsValidUpload(command.ImageStream, command.ContentType))
            return Error.Validation("PhotoFrame.InvalidFile",
                "Only JPG and PNG files up to 10 MB are accepted as sponsor logo images.");

        var config = await _db.PhotoFrameConfigs
            .Include(c => c.EventTool).ThenInclude(t => t.Event)
            .Include(c => c.LandingPage)
            .FirstOrDefaultAsync(c => c.Id == command.PhotoFrameConfigId, ct);

        if (config is null)
            return Error.NotFound("PhotoFrame.NotFound", "Photo frame config not found.");

        if (config.EventTool.Event.OwnerId != userId)
            return Error.Forbidden("PhotoFrame.Forbidden", "You do not own this frame config.");

        if (!string.IsNullOrWhiteSpace(config.SponsorLogoPath))
            await _storage.DeleteAsync(config.SponsorLogoPath, ct);

        command.ImageStream.Position = 0;
        var storagePath = await _storage.SaveAsync(
            command.ImageStream, command.FileName, "sponsor-logos", ct);

        config.SetSponsorLogo(storagePath);
        await _uow.SaveChangesAsync(ct);

        return config.ToSetupDto(_storage);
    }
}
