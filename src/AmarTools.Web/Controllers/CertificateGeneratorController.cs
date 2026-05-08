using AmarTools.Modules.CertificateGenerator.Commands.SaveCertificateMappings;
using AmarTools.Modules.CertificateGenerator.Commands.GenerateCertificateBatch;
using AmarTools.Modules.CertificateGenerator.Commands.ProcessCertificateBatch;
using AmarTools.Modules.CertificateGenerator.Commands.UploadBaseTemplate;
using AmarTools.Modules.CertificateGenerator.Commands.UploadRecipientDataset;
using AmarTools.Modules.CertificateGenerator.Contracts;
using AmarTools.Modules.CertificateGenerator.Queries.DownloadCertificateBatch;
using AmarTools.Modules.CertificateGenerator.Queries.GetCertificateTemplateSetup;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmarTools.Web.Controllers;

[Authorize]
[Route("api/certificate-generator")]
[ApiController]
public sealed class CertificateGeneratorController : ApiControllerBase
{
    private readonly ISender _sender;

    public CertificateGeneratorController(ISender sender) => _sender = sender;

    [HttpGet("setup/{eventToolId:guid}")]
    [ProducesResponseType(typeof(CertificateTemplateSetupDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSetup(Guid eventToolId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetCertificateTemplateSetupQuery(eventToolId), ct);
        return Ok(result);
    }

    [HttpPost("{certificateTemplateConfigId:guid}/base-template")]
    [ProducesResponseType(typeof(CertificateTemplateSetupDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> UploadBaseTemplate(
        Guid certificateTemplateConfigId,
        [FromForm] UploadCertificateTemplateRequest request,
        CancellationToken ct)
    {
        if (request.Template is null || request.Template.Length == 0)
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Certificates.TemplateRequired",
                Detail = "Please provide a certificate template file."
            });

        await using var stream = request.Template.OpenReadStream();

        var result = await _sender.Send(
            new UploadBaseTemplateCommand(
                certificateTemplateConfigId,
                stream,
                request.Template.FileName,
                request.Template.ContentType),
            ct);

        return Ok(result);
    }

    [HttpPut("{certificateTemplateConfigId:guid}/mappings")]
    [ProducesResponseType(typeof(CertificateTemplateSetupDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> SaveMappings(
        Guid certificateTemplateConfigId,
        [FromBody] SaveCertificateMappingsRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new SaveCertificateMappingsCommand(
                certificateTemplateConfigId,
                request.Mappings,
                request.OutputFileNamePattern),
            ct);

        return Ok(result);
    }

    [HttpPost("{certificateTemplateConfigId:guid}/recipient-dataset")]
    [ProducesResponseType(typeof(CertificateDatasetPreviewDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> UploadRecipientDataset(
        Guid certificateTemplateConfigId,
        [FromForm] UploadRecipientDatasetRequest request,
        CancellationToken ct)
    {
        if (request.Dataset is null || request.Dataset.Length == 0)
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Certificates.DatasetRequired",
                Detail = "Please provide a recipient dataset file."
            });

        await using var stream = request.Dataset.OpenReadStream();

        var result = await _sender.Send(
            new UploadRecipientDatasetCommand(
                certificateTemplateConfigId,
                stream,
                request.Dataset.FileName),
            ct);

        return Ok(result);
    }

    [HttpPost("{certificateTemplateConfigId:guid}/generate-batch")]
    [ProducesResponseType(typeof(CertificateGenerationBatchDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> GenerateBatch(
        Guid certificateTemplateConfigId,
        [FromBody] GenerateCertificateBatchRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new GenerateCertificateBatchCommand(
                certificateTemplateConfigId,
                request.OutputFormat),
            ct);

        return Ok(result);
    }

    [HttpPost("{certificateTemplateConfigId:guid}/batches/{batchId:guid}/process")]
    [ProducesResponseType(typeof(CertificateGenerationBatchDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> ProcessBatch(
        Guid certificateTemplateConfigId,
        Guid batchId,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new ProcessCertificateBatchCommand(certificateTemplateConfigId, batchId),
            ct);

        return Ok(result);
    }

    [HttpGet("{certificateTemplateConfigId:guid}/batches/{batchId:guid}/download")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> DownloadBatch(
        Guid certificateTemplateConfigId,
        Guid batchId,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new DownloadCertificateBatchQuery(certificateTemplateConfigId, batchId),
            ct);

        if (result.IsFailure)
            return MapError(result.Error);

        var dto = result.Value;
        return File(dto.FileStream, dto.ContentType, dto.FileName);
    }
}

public sealed record UploadCertificateTemplateRequest(IFormFile Template);

public sealed record UploadRecipientDatasetRequest(IFormFile Dataset);

public sealed record SaveCertificateMappingsRequest(
    IReadOnlyCollection<CertificateFieldMappingInputDto> Mappings,
    string? OutputFileNamePattern = null
);

public sealed record GenerateCertificateBatchRequest(string OutputFormat = "pdf");
