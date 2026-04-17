using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.PhotoFrame.Commands.SetupPhotoFrame;

internal sealed class SetupPhotoFrameHandler
    : IRequestHandler<SetupPhotoFrameCommand, Result<PhotoFrameSetupDto>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork         _uow;
    private readonly IFileStorageService _storage;

    public SetupPhotoFrameHandler(
        AppDbContext db, ICurrentUserService currentUser,
        IUnitOfWork uow, IFileStorageService storage)
    {
        _db          = db;
        _currentUser = currentUser;
        _uow         = uow;
        _storage     = storage;
    }

    public async Task<Result<PhotoFrameSetupDto>> Handle(
        SetupPhotoFrameCommand command, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        // ── Verify the EventTool exists, belongs to the caller, and is PhotoFrame ──
        var eventTool = await _db.EventTools
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == command.EventToolId, ct);

        if (eventTool is null)
            return Error.NotFound("EventTool.NotFound", "Event tool not found.");

        if (eventTool.Event.OwnerId != userId)
            return Error.Forbidden("EventTool.Forbidden", "You do not own this event.");

        if (eventTool.ToolType != ToolType.EventPhotoframeGenerator)
            return Error.Validation("EventTool.WrongType",
                "This tool is not an EventPhotoframeGenerator.");

        // ── Upsert config (idempotent) ─────────────────────────────────────────
        var config = await _db.PhotoFrameConfigs
            .Include(c => c.LandingPage)
            .FirstOrDefaultAsync(c => c.EventToolId == command.EventToolId, ct);

        if (config is null)
        {
            config = PhotoFrameConfig.Create(command.EventToolId, command.EventName);
            _db.PhotoFrameConfigs.Add(config);
        }

        // Always apply all fields — Create() only sets EventName and the slug
        config.UpdateDetails(
            command.EventName, command.SponsorName,
            command.VenueName, command.EventDateTime);

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.Message;
            for (var inner = ex.InnerException; inner is not null; inner = inner.InnerException)
                msg = inner.Message;
            return Error.Failure("PhotoFrame.SaveFailed", msg);
        }

        return config.ToSetupDto(_storage);
    }
}
