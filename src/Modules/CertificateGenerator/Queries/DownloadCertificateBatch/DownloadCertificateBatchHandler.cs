using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AmarTools.Modules.CertificateGenerator.Queries.DownloadCertificateBatch;

internal sealed class DownloadCertificateBatchHandler
    : IRequestHandler<DownloadCertificateBatchQuery, Result<CertificateBatchDownloadDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _storage;

    public DownloadCertificateBatchHandler(
        AppDbContext db,
        ICurrentUserService currentUser,
        IFileStorageService storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<Result<CertificateBatchDownloadDto>> Handle(
        DownloadCertificateBatchQuery query,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var batch = await _db.CertificateGenerationBatches
            .Include(b => b.CertificateTemplateConfig)
                .ThenInclude(c => c.EventTool)
                    .ThenInclude(t => t.Event)
            .FirstOrDefaultAsync(b =>
                b.Id == query.BatchId &&
                b.CertificateTemplateConfigId == query.CertificateTemplateConfigId, ct);

        if (batch is null)
            return Error.NotFound("Certificates.BatchNotFound", "Generation batch not found.");

        if (batch.CertificateTemplateConfig.EventTool.Event.OwnerId != userId)
            return Error.Forbidden("Certificates.Forbidden", "You do not own this batch.");

        if (batch.Status != "completed" || string.IsNullOrWhiteSpace(batch.OutputFilePath))
            return Error.Validation("Certificates.NotReady",
                "Batch output is not available. Process the batch first.");

        var stream = await _storage.OpenReadAsync(batch.OutputFilePath, ct);

        var ext = Path.GetExtension(batch.OutputFilePath)?.TrimStart('.').ToLowerInvariant();
        var (contentType, fileName) = ext switch
        {
            "zip" => ("application/zip", $"certificates_{batch.Id:N}.zip"),
            "pptx" => ("application/vnd.openxmlformats-officedocument.presentationml.presentation",
                       $"certificates_{batch.Id:N}.pptx"),
            _ => ("application/octet-stream", $"certificates_{batch.Id:N}")
        };

        return new CertificateBatchDownloadDto(stream, fileName, contentType);
    }
}
