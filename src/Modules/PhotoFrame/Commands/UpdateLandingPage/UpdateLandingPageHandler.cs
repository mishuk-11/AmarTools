using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.PhotoFrame.Commands.UpdateLandingPage;

internal sealed class UpdateLandingPageHandler
    : IRequestHandler<UpdateLandingPageCommand, Result<PhotoFrameSetupDto>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _uow;
    private readonly IFileStorageService _storage;

    public UpdateLandingPageHandler(
        AppDbContext db, ICurrentUserService currentUser,
        IUnitOfWork uow, IFileStorageService storage)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
        _storage     = storage;
    }

    public async Task<Result<PhotoFrameSetupDto>> Handle(
        UpdateLandingPageCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var config = await _db.PhotoFrameConfigs
            .Include(c => c.EventTool).ThenInclude(t => t.Event)
            .Include(c => c.LandingPage)
            .FirstOrDefaultAsync(c => c.Id == command.PhotoFrameConfigId, ct);

        if (config is null)
            return Error.NotFound("PhotoFrame.NotFound", "Photo frame config not found.");

        if (config.EventTool.Event.OwnerId != userId)
            return Error.Forbidden("PhotoFrame.Forbidden", "You do not own this frame config.");

        // ── Upsert landing page ───────────────────────────────────────────────
        if (config.LandingPage is null)
        {
            var lp = LandingPageConfig.CreateDefault(config.Id);
            lp.Update(
                command.TemplateName,
                command.BackgroundColor,
                backgroundImagePath: null,
                command.HeadlineText,
                command.InstructionText,
                command.DownloadButtonText);
            _db.LandingPageConfigs.Add(lp);
        }
        else
        {
            config.LandingPage.Update(
                command.TemplateName,
                command.BackgroundColor,
                config.LandingPage.BackgroundImagePath,
                command.HeadlineText,
                command.InstructionText,
                command.DownloadButtonText);
        }

        // ── Publish / unpublish ───────────────────────────────────────────────
        if (command.Publish)
        {
            var publishResult = config.Publish();
            if (publishResult.IsFailure) return publishResult.Error;
        }
        else
        {
            config.Unpublish();
        }

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // Walk the inner-exception chain to get the real DB error (e.g. constraint name)
            var msg = ex.Message;
            for (var inner = ex.InnerException; inner is not null; inner = inner.InnerException)
                msg = inner.Message;
            return Error.Failure("PhotoFrame.SaveFailed", msg);
        }

        // Reload to ensure LandingPage navigation is populated after Add
        await _db.Entry(config).ReloadAsync(ct);
        await _db.Entry(config).Reference(c => c.LandingPage).LoadAsync(ct);

        return config.ToSetupDto(_storage);
    }
}
