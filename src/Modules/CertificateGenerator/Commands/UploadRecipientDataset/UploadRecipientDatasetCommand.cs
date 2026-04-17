using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;

namespace AmarTools.Modules.CertificateGenerator.Commands.UploadRecipientDataset;

public sealed record UploadRecipientDatasetCommand(
    Guid CertificateTemplateConfigId,
    Stream DatasetStream,
    string FileName
) : IRequest<Result<CertificateDatasetPreviewDto>>;
