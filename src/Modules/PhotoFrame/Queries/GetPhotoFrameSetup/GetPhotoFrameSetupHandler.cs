using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.PhotoFrame.Queries.GetPhotoFrameSetup;

internal sealed class GetPhotoFrameSetupHandler
    : IRequestHandler<GetPhotoFrameSetupQuery, Result<PhotoFrameSetupDto>>
{
    private readonly AppDbContext        _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _storage;

    public GetPhotoFrameSetupHandler(
        AppDbContext db, ICurrentUserService currentUser, IFileStorageService storage)
    {
        _db          = db;
        _currentUser = currentUser;
        _storage     = storage;
    }

    public async Task<Result<PhotoFrameSetupDto>> Handle(
        GetPhotoFrameSetupQuery query, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var config = await _db.PhotoFrameConfigs
            .AsNoTracking()
            .Include(c => c.EventTool).ThenInclude(t => t.Event)
            .Include(c => c.LandingPage)
            .FirstOrDefaultAsync(c => c.EventToolId == query.EventToolId, ct);

        if (config is null)
            return Error.NotFound("PhotoFrame.NotFound",
                "No photo frame config found for this event tool.");

        if (config.EventTool.Event.OwnerId != userId)
            return Error.Forbidden("PhotoFrame.Forbidden", "You do not own this frame config.");

        return config.ToSetupDto(_storage);
    }
}
