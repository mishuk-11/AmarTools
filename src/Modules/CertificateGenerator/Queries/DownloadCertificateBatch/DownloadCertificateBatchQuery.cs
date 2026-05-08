using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;

namespace AmarTools.Modules.CertificateGenerator.Queries.DownloadCertificateBatch;

public sealed record DownloadCertificateBatchQuery(
    Guid CertificateTemplateConfigId,
    Guid BatchId
) : IRequest<Result<CertificateBatchDownloadDto>>;
