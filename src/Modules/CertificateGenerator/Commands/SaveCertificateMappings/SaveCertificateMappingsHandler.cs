using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.CertificateGenerator.Commands.SaveCertificateMappings;

internal sealed class SaveCertificateMappingsHandler
    : IRequestHandler<SaveCertificateMappingsCommand, Result<CertificateTemplateSetupDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;

    public SaveCertificateMappingsHandler(
        AppDbContext db,
        ICurrentUserService currentUser,
        IUnitOfWork uow,
        IFileStorageService storage)
    {
        _db = db;
        _currentUser = currentUser;
        _uow = uow;
        _storage = storage;
    }

    public async Task<Result<CertificateTemplateSetupDto>> Handle(
        SaveCertificateMappingsCommand command,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var config = await _db.CertificateTemplateConfigs
            .Include(c => c.EventTool).ThenInclude(t => t.Event)
            .Include(c => c.FieldMappings)
            .FirstOrDefaultAsync(c => c.Id == command.CertificateTemplateConfigId, ct);

        if (config is null)
            return Error.NotFound("Certificates.NotFound", "Certificate template config not found.");

        if (config.EventTool.Event.OwnerId != userId)
            return Error.Forbidden("Certificates.Forbidden", "You do not own this certificate setup.");

        if (command.Mappings.Count == 0)
            return Error.Validation("Certificates.MappingsRequired",
                "At least one field mapping must be provided.");

        _db.CertificateFieldMappings.RemoveRange(config.FieldMappings);

        var newMappings = command.Mappings.Select(m => CertificateFieldMapping.Create(
            config.Id,
            m.FieldKey,
            m.SourceColumn,
            m.FieldType,
            m.PositionX,
            m.PositionY,
            m.Width,
            m.Height,
            m.FontSize,
            m.FontColor));

        foreach (var mapping in newMappings)
            config.FieldMappings.Add(mapping);

        await _uow.SaveChangesAsync(ct);
        return config.ToSetupDto(_storage);
    }
}
