using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.CertificateGenerator.Contracts;
using AmarTools.Modules.CertificateGenerator.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.CertificateGenerator.Queries.GetCertificateTemplateSetup;

internal sealed class GetCertificateTemplateSetupHandler
    : IRequestHandler<GetCertificateTemplateSetupQuery, Result<CertificateTemplateSetupDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _storage;
    private readonly IPptxPlaceholderExtractor _placeholderExtractor;
    private readonly IUnitOfWork _uow;

    public GetCertificateTemplateSetupHandler(
        AppDbContext db,
        ICurrentUserService currentUser,
        IFileStorageService storage,
        IPptxPlaceholderExtractor placeholderExtractor,
        IUnitOfWork uow)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
        _placeholderExtractor = placeholderExtractor;
        _uow = uow;
    }

    public async Task<Result<CertificateTemplateSetupDto>> Handle(
        GetCertificateTemplateSetupQuery query,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var eventTool = await _db.EventTools
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == query.EventToolId, ct);

        if (eventTool is null)
            return Error.NotFound("EventTool.NotFound", "Event tool not found.");

        if (eventTool.Event.OwnerId != userId)
            return Error.Forbidden("Certificates.Forbidden", "You do not own this event.");

        if (eventTool.ToolType != ToolType.CertificateGenerator)
            return Error.Validation("EventTool.WrongType", "This tool is not a CertificateGenerator.");

        var config = await _db.CertificateTemplateConfigs
            .Include(c => c.FieldMappings)
            .FirstOrDefaultAsync(c => c.EventToolId == query.EventToolId, ct);

        if (config is null)
        {
            config = CertificateTemplateConfig.Create(query.EventToolId);
            _db.CertificateTemplateConfigs.Add(config);
            await _uow.SaveChangesAsync(ct);
        }

        IReadOnlyList<string>? detectedPlaceholders = null;
        if (!string.IsNullOrWhiteSpace(config.BaseTemplatePath) &&
            string.Equals(config.BaseTemplateFileType, "pptx", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await using var templateStream = await _storage.OpenReadAsync(config.BaseTemplatePath, ct);
                detectedPlaceholders = _placeholderExtractor.Extract(templateStream);
            }
            catch { }
        }

        return config.ToSetupDto(_storage, detectedPlaceholders);
    }
}
