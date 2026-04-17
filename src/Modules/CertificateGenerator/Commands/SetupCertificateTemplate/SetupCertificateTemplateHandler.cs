using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Domain.Enums;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.CertificateGenerator.Commands.SetupCertificateTemplate;

internal sealed class SetupCertificateTemplateHandler
    : IRequestHandler<SetupCertificateTemplateCommand, Result<CertificateTemplateSetupDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;

    public SetupCertificateTemplateHandler(
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
        SetupCertificateTemplateCommand command,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var eventTool = await _db.EventTools
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == command.EventToolId, ct);

        if (eventTool is null)
            return Error.NotFound("EventTool.NotFound", "Event tool not found.");

        if (eventTool.Event.OwnerId != userId)
            return Error.Forbidden("EventTool.Forbidden", "You do not own this event.");

        if (eventTool.ToolType != ToolType.CertificateGenerator)
            return Error.Validation("EventTool.WrongType",
                "This tool is not a CertificateGenerator.");

        var config = await _db.CertificateTemplateConfigs
            .Include(c => c.FieldMappings)
            .FirstOrDefaultAsync(c => c.EventToolId == command.EventToolId, ct);

        if (config is null)
        {
            config = CertificateTemplateConfig.Create(command.EventToolId, command.TemplateName);
            _db.CertificateTemplateConfigs.Add(config);
        }

        config.UpdateMetadata(command.TemplateName, command.EmailSubject, command.EmailBody);

        await _uow.SaveChangesAsync(ct);
        return config.ToSetupDto(_storage);
    }
}
