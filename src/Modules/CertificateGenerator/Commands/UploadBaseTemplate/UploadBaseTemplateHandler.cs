using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.CertificateGenerator.Commands.UploadBaseTemplate;

internal sealed class UploadBaseTemplateHandler
    : IRequestHandler<UploadBaseTemplateCommand, Result<CertificateTemplateSetupDto>>
{
    private static readonly string[] AllowedExtensions = [".pdf", ".png", ".jpg", ".jpeg", ".eps"];

    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;

    public UploadBaseTemplateHandler(
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
        UploadBaseTemplateCommand command,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;
        var extension = Path.GetExtension(command.FileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            return Error.Validation("Certificates.InvalidTemplateFile",
                "Only PDF, PNG, JPG, and EPS files are supported as base certificate templates.");

        var config = await _db.CertificateTemplateConfigs
            .Include(c => c.EventTool).ThenInclude(t => t.Event)
            .Include(c => c.FieldMappings)
            .FirstOrDefaultAsync(c => c.Id == command.CertificateTemplateConfigId, ct);

        if (config is null)
            return Error.NotFound("Certificates.NotFound", "Certificate template config not found.");

        if (config.EventTool.Event.OwnerId != userId)
            return Error.Forbidden("Certificates.Forbidden", "You do not own this certificate setup.");

        if (!string.IsNullOrWhiteSpace(config.BaseTemplatePath))
            await _storage.DeleteAsync(config.BaseTemplatePath, ct);

        command.TemplateStream.Position = 0;
        var storagePath = await _storage.SaveAsync(
            command.TemplateStream,
            command.FileName,
            "certificate-templates",
            ct);

        config.SetBaseTemplate(storagePath, command.FileName, extension.TrimStart('.'));

        await _uow.SaveChangesAsync(ct);
        return config.ToSetupDto(_storage);
    }
}
