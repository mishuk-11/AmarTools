using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.CertificateGenerator.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.CertificateGenerator.Commands.UploadRecipientDataset;

internal sealed class UploadRecipientDatasetHandler
    : IRequestHandler<UploadRecipientDatasetCommand, Result<Contracts.CertificateDatasetPreviewDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;
    private readonly ICertificateDatasetParser _datasetParser;

    public UploadRecipientDatasetHandler(
        AppDbContext db,
        ICurrentUserService currentUser,
        IUnitOfWork uow,
        IFileStorageService storage,
        ICertificateDatasetParser datasetParser)
    {
        _db = db;
        _currentUser = currentUser;
        _uow = uow;
        _storage = storage;
        _datasetParser = datasetParser;
    }

    public async Task<Result<Contracts.CertificateDatasetPreviewDto>> Handle(
        UploadRecipientDatasetCommand command,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var config = await _db.CertificateTemplateConfigs
            .Include(c => c.EventTool).ThenInclude(t => t.Event)
            .FirstOrDefaultAsync(c => c.Id == command.CertificateTemplateConfigId, ct);

        if (config is null)
            return Error.NotFound("Certificates.NotFound", "Certificate template config not found.");

        if (config.EventTool.Event.OwnerId != userId)
            return Error.Forbidden("Certificates.Forbidden", "You do not own this certificate setup.");

        var parseResult = await _datasetParser.ParsePreviewAsync(command.DatasetStream, command.FileName, ct);
        if (parseResult.IsFailure)
            return parseResult.Error;

        if (!string.IsNullOrWhiteSpace(config.RecipientDatasetPath))
            await _storage.DeleteAsync(config.RecipientDatasetPath, ct);

        command.DatasetStream.Position = 0;
        var storagePath = await _storage.SaveAsync(
            command.DatasetStream,
            command.FileName,
            "certificate-datasets",
            ct);

        config.SetRecipientDataset(storagePath, command.FileName);
        await _uow.SaveChangesAsync(ct);

        return parseResult.Value;
    }
}
