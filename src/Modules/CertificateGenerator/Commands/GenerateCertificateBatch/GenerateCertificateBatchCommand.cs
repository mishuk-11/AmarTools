using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;

namespace AmarTools.Modules.CertificateGenerator.Commands.GenerateCertificateBatch;

public sealed record GenerateCertificateBatchCommand(
    Guid CertificateTemplateConfigId,
    string OutputFormat
) : IRequest<Result<CertificateGenerationBatchDto>>;
