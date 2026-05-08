using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;

namespace AmarTools.Modules.CertificateGenerator.Commands.ProcessCertificateBatch;

public sealed record ProcessCertificateBatchCommand(
    Guid CertificateTemplateConfigId,
    Guid BatchId
) : IRequest<Result<CertificateGenerationBatchDto>>;
