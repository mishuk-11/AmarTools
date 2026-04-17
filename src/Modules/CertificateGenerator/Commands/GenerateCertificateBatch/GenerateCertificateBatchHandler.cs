using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;
using AmarTools.Infrastructure.Persistence;
using AmarTools.Modules.CertificateGenerator.Contracts;
using AmarTools.Modules.CertificateGenerator.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AmarTools.Modules.CertificateGenerator.Commands.GenerateCertificateBatch;

internal sealed class GenerateCertificateBatchHandler
    : IRequestHandler<GenerateCertificateBatchCommand, Result<CertificateGenerationBatchDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;
    private readonly ICertificateDatasetParser _datasetParser;

    public GenerateCertificateBatchHandler(
        AppDbContext db,
        ICurrentUserService currentUser,
        IUnitOfWork uow,
        IFileStorageService storage,
        ICertificateDatasetParser datasetParser)
    {
        _db = db;
        _currentUser = currentUser;
        _uow = uow;
        _storage = storage;
        _datasetParser = datasetParser;
    }

    public async Task<Result<CertificateGenerationBatchDto>> Handle(
        GenerateCertificateBatchCommand command,
        CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Error.Unauthorized("Auth.Required", "You must be logged in.");

        var userId = _currentUser.UserId.Value;

        var config = await _db.CertificateTemplateConfigs
            .Include(c => c.EventTool).ThenInclude(t => t.Event)
            .Include(c => c.FieldMappings)
            .FirstOrDefaultAsync(c => c.Id == command.CertificateTemplateConfigId, ct);

        if (config is null)
            return Error.NotFound("Certificates.NotFound", "Certificate template config not found.");

        if (config.EventTool.Event.OwnerId != userId)
            return Error.Forbidden("Certificates.Forbidden", "You do not own this certificate setup.");

        if (string.IsNullOrWhiteSpace(config.BaseTemplatePath))
            return Error.Validation("Certificates.BaseTemplateRequired",
                "Upload a base certificate template before generating a batch.");

        if (string.IsNullOrWhiteSpace(config.RecipientDatasetPath) ||
            string.IsNullOrWhiteSpace(config.RecipientDatasetFileName))
            return Error.Validation("Certificates.DatasetRequired",
                "Upload a recipient dataset before generating a batch.");

        if (config.FieldMappings.Count == 0)
            return Error.Validation("Certificates.MappingsRequired",
                "Save at least one field mapping before generating a batch.");

        await using var datasetStream = await _storage.OpenReadAsync(config.RecipientDatasetPath, ct);
        var parsedResult = await _datasetParser.ParseRowsAsync(datasetStream, config.RecipientDatasetFileName, ct);
        if (parsedResult.IsFailure)
            return parsedResult.Error;

        var parsed = parsedResult.Value;

        var missingColumns = config.FieldMappings
            .Select(m => m.SourceColumn)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(column => !parsed.Columns.Contains(column, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (missingColumns.Count > 0)
            return Error.Validation("Certificates.DatasetColumnsMissing",
                $"The uploaded dataset is missing required mapped columns: {string.Join(", ", missingColumns)}");

        if (parsed.Rows.Count == 0)
            return Error.Validation("Certificates.EmptyDatasetRows",
                "The uploaded dataset does not contain any recipient rows.");

        var batch = CertificateGenerationBatch.Create(
            config.Id,
            string.IsNullOrWhiteSpace(command.OutputFormat) ? "pdf" : command.OutputFormat,
            parsed.Rows.Count);

        _db.CertificateGenerationBatches.Add(batch);

        for (var i = 0; i < parsed.Rows.Count; i++)
        {
            var row = parsed.Rows[i];
            var recipientName = ResolveRecipientName(row);
            if (string.IsNullOrWhiteSpace(recipientName))
                recipientName = $"Recipient {i + 1}";

            var recipientEmail = ResolveRecipientEmail(row);
            var payloadJson = JsonSerializer.Serialize(row);

            batch.Items.Add(CertificateGenerationItem.Create(
                batch.Id,
                i + 1,
                recipientName,
                recipientEmail,
                payloadJson));
        }

        await _uow.SaveChangesAsync(ct);

        await _db.Entry(batch).Collection(b => b.Items).LoadAsync(ct);
        return batch.ToDto();
    }

    private static string? ResolveRecipientName(IReadOnlyDictionary<string, string> row)
    {
        var candidates = new[] { "Name", "FullName", "Full Name", "RecipientName", "Recipient Name" };

        foreach (var candidate in candidates)
        {
            if (row.TryGetValue(candidate, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;
        }

        return row.Values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
    }

    private static string? ResolveRecipientEmail(IReadOnlyDictionary<string, string> row)
    {
        var candidates = new[] { "Email", "EmailAddress", "Email Address" };

        foreach (var candidate in candidates)
        {
            if (row.TryGetValue(candidate, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }
}
