using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.CertificateGenerator.Queries.GetCertificateTemplateSetup;

internal sealed class GetCertificateTemplateSetupHandler
    : IRequestHandler<GetCertificateTemplateSetupQuery, Result<CertificateTemplateSetupDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _storage;

    public GetCertificateTemplateSetupHandler(
        AppDbContext db,
        ICurrentUserService currentUser,
        IFileStorageService storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<Result<CertificateTemplateSetupDto>> Handle(
        GetCertificateTemplateSetupQuery query,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var config = await _db.CertificateTemplateConfigs
            .AsNoTracking()
            .Include(c => c.EventTool).ThenInclude(t => t.Event)
            .Include(c => c.FieldMappings)
            .FirstOrDefaultAsync(c => c.EventToolId == query.EventToolId, ct);

        if (config is null)
            return Error.NotFound("Certificates.NotFound",
                "No certificate template config found for this event tool.");

        if (config.EventTool.Event.OwnerId != userId)
            return Error.Forbidden("Certificates.Forbidden", "You do not own this certificate setup.");

        return config.ToSetupDto(_storage);
    }
}
