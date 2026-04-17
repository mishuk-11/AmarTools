using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;

namespace AmarTools.Modules.CertificateGenerator.Commands.UploadBaseTemplate;

public sealed record UploadBaseTemplateCommand(
    Guid CertificateTemplateConfigId,
    Stream TemplateStream,
    string FileName,
    string ContentType
) : IRequest<Result<CertificateTemplateSetupDto>>;
