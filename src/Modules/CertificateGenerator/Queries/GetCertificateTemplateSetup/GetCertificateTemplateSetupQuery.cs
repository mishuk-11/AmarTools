using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;

namespace AmarTools.Modules.CertificateGenerator.Queries.GetCertificateTemplateSetup;

public sealed record GetCertificateTemplateSetupQuery(Guid EventToolId)
    : IRequest<Result<CertificateTemplateSetupDto>>;
