using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.CertificateGenerator.Contracts;
using MediatR;

namespace AmarTools.Modules.CertificateGenerator.Commands.SaveCertificateMappings;

public sealed record SaveCertificateMappingsCommand(
    Guid CertificateTemplateConfigId,
    IReadOnlyCollection<CertificateFieldMappingInputDto> Mappings,
    string? OutputFileNamePattern = null
) : IRequest<Result<CertificateTemplateSetupDto>>;
