using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.CertificateGenerator.Contracts;
using AmarTools.Modules.CertificateGenerator.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AmarTools.Modules.CertificateGenerator.Commands.ProcessCertificateBatch;

internal sealed partial class ProcessCertificateBatchHandler
    : IRequestHandler<ProcessCertificateBatchCommand, Result<CertificateGenerationBatchDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;
    private readonly IPptxCertificateRenderer _renderer;
    private readonly IPdfConverter _pdfConverter;

    public ProcessCertificateBatchHandler(
        AppDbContext db,
        ICurrentUserService currentUser,
        IUnitOfWork uow,
        IFileStorageService storage,
        IPptxCertificateRenderer renderer,
        IPdfConverter pdfConverter)
    {
        _db = db;
        _currentUser = currentUser;
        _uow = uow;
        _storage = storage;
        _renderer = renderer;
        _pdfConverter = pdfConverter;
    }

    public async Task<Result<CertificateGenerationBatchDto>> Handle(
        ProcessCertificateBatchCommand command,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var batch = await _db.CertificateGenerationBatches
            .Include(b => b.CertificateTemplateConfig)
                .ThenInclude(c => c.EventTool)
                    .ThenInclude(t => t.Event)
            .Include(b => b.CertificateTemplateConfig)
                .ThenInclude(c => c.FieldMappings)
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b =>
                b.Id == command.BatchId &&
                b.CertificateTemplateConfigId == command.CertificateTemplateConfigId, ct);

        if (batch is null)
            return Error.NotFound("Certificates.BatchNotFound", "Generation batch not found.");

        var config = batch.CertificateTemplateConfig;

        if (config.EventTool.Event.OwnerId != userId)
            return Error.Forbidden("Certificates.Forbidden", "You do not own this batch.");

        if (batch.Status != "pending")
            return Error.Conflict("Certificates.BatchNotPending",
                $"Batch is already '{batch.Status}' and cannot be reprocessed.");

        if (string.IsNullOrWhiteSpace(config.BaseTemplatePath))
            return Error.Validation("Certificates.TemplateRequired", "No base template uploaded.");

        if (!string.Equals(config.BaseTemplateFileType, "pptx", StringComparison.OrdinalIgnoreCase))
            return Error.Validation("Certificates.PptxOnly",
                "Only PPTX templates are supported for batch generation. Please upload a .pptx file.");

        batch.MarkProcessing();

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch
        {
            return Error.Failure("Certificates.SaveFailed",
                "Could not update batch status. Please try again.");
        }

        try
        {
            var mappings = config.FieldMappings.ToList();
            var items = batch.Items.OrderBy(i => i.SequenceNumber).ToList();

            // Load template once into memory for reuse across all recipients
            using var templateBuffer = new MemoryStream();
            await using (var templateStream = await _storage.OpenReadAsync(config.BaseTemplatePath, ct))
                await templateStream.CopyToAsync(templateBuffer, ct);

            var toPdf   = string.Equals(batch.OutputFormat, "pdf", StringComparison.OrdinalIgnoreCase);
            var fileExt = toPdf ? _pdfConverter.OutputExtension : "pptx";

            using var zipStream = new MemoryStream();
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var item in items)
                {
                    var row = JsonSerializer.Deserialize<Dictionary<string, string>>(
                        item.PayloadJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? [];

                    try
                    {
                        templateBuffer.Position = 0;
                        using var rendered = await _renderer.RenderSingleAsync(
                            templateBuffer, row, mappings, ct);

                        var fileName = ResolveFileName(
                            config.OutputFileNamePattern, row, mappings, item.SequenceNumber, fileExt);

                        var entry = zip.CreateEntry(fileName, CompressionLevel.Optimal);
                        await using var entryStream = entry.Open();

                        if (toPdf)
                        {
                            using var pdfStream = await _pdfConverter.ConvertFromPptxAsync(rendered, ct);
                            pdfStream.Position = 0;
                            await pdfStream.CopyToAsync(entryStream, ct);
                        }
                        else
                        {
                            rendered.Position = 0;
                            await rendered.CopyToAsync(entryStream, ct);
                        }

                        item.MarkCompleted();
                        batch.IncrementCompleted();
                    }
                    catch
                    {
                        item.MarkFailed();
                        batch.IncrementFailed();
                    }
                }
            }

            zipStream.Position = 0;
            var zipFileName = $"certificates_{batch.Id:N}.zip";
            var outputPath = await _storage.SaveAsync(zipStream, zipFileName, "certificate-outputs", ct);

            batch.MarkCompleted(outputPath, batch.CompletedRecipients);
            await _uow.SaveChangesAsync(ct);
        }
        catch (Exception)
        {
            // Detach all modified entries except the batch itself to avoid cascading failures
            foreach (var entry in _db.ChangeTracker.Entries().ToList())
                if (entry.Entity != (object)batch)
                    entry.State = EntityState.Detached;

            batch.MarkFailed();
            try { await _uow.SaveChangesAsync(ct); } catch { /* best-effort */ }

            return Error.Failure("Certificates.RenderFailed",
                "Certificate generation failed. Verify your PPTX template is valid and try again.");
        }

        return batch.ToDto(_storage);
    }

    private static string ResolveFileName(
        string? pattern,
        IReadOnlyDictionary<string, string> row,
        IReadOnlyList<CertificateFieldMapping> mappings,
        int sequenceNumber,
        string extension)
    {
        string name;

        if (string.IsNullOrWhiteSpace(pattern))
        {
            name = $"certificate_{sequenceNumber}";
        }
        else
        {
            name = pattern;
            foreach (var mapping in mappings)
            {
                var placeholder = $"<<{mapping.FieldKey}>>";
                if (!name.Contains(placeholder, StringComparison.OrdinalIgnoreCase)) continue;
                var value = row.TryGetValue(mapping.SourceColumn, out var v) ? v ?? string.Empty : string.Empty;
                name = name.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
            }

            // Remove any unreplaced placeholders
            name = UnresolvedPlaceholder().Replace(name, string.Empty);
        }

        // Sanitize for use as a file name
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        name = name.Trim('_', ' ', '.');

        if (string.IsNullOrWhiteSpace(name))
            name = $"certificate_{sequenceNumber}";

        return $"{name}.{extension}";
    }

    [GeneratedRegex(@"<<[^<>]+>>", RegexOptions.IgnoreCase)]
    private static partial Regex UnresolvedPlaceholder();
}
