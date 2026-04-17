using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.PhotoFrame.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.PhotoFrame.Queries.GetLandingPage;

/// <summary>
/// Public handler — no authentication check.
/// Returns the landing page data needed by the guest React page.
/// </summary>
internal sealed class GetLandingPageHandler
    : IRequestHandler<GetLandingPageQuery, Result<LandingPageDto>>
{
    private readonly AppDbContext        _db;
    private readonly IFileStorageService _storage;

    public GetLandingPageHandler(AppDbContext db, IFileStorageService storage)
    {
        _db      = db;
        _storage = storage;
    }

    public async Task<Result<LandingPageDto>> Handle(
        GetLandingPageQuery query, CancellationToken ct)
    {
        var config = await _db.PhotoFrameConfigs
            .AsNoTracking()
            .Include(c => c.LandingPage)
            .FirstOrDefaultAsync(c => c.SharingSlug == query.SharingSlug, ct);

        if (config is null || !config.IsPublished)
            return Error.NotFound("PhotoFrame.NotFound",
                "This photo frame event could not be found.");

        if (config.LandingPage is null)
            return Error.Failure("PhotoFrame.NotConfigured",
                "This event's landing page has not been configured yet.");

        return config.LandingPage.ToLandingPageDto(config, _storage);
    }
}
