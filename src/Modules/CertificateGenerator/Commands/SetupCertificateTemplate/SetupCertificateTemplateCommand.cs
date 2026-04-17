using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;

namespace AmarTools.Modules.CertificateGenerator.Commands.SetupCertificateTemplate;

public sealed record SetupCertificateTemplateCommand(
    Guid EventToolId,
    string TemplateName,
    string? EmailSubject,
    string? EmailBody
) : IRequest<Result<CertificateTemplateSetupDto>>;
