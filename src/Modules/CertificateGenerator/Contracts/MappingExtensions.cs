using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;

namespace AmarTools.Modules.CertificateGenerator.Contracts;

internal static class MappingExtensions
{
    internal static CertificateTemplateSetupDto ToSetupDto(
        this CertificateTemplateConfig config,
        IFileStorageService storage,
        IReadOnlyList<string>? detectedPlaceholders = null) =>
        new(
            config.Id,
            config.EventToolId,
            config.TemplateName,
            BaseTemplateUrl: config.BaseTemplatePath is not null
                ? storage.GetPublicUrl(config.BaseTemplatePath)
                : null,
            config.BaseTemplateFileName,
            config.BaseTemplateFileType,
            config.RecipientDatasetFileName,
            config.FieldMappings
                .OrderBy(m => m.CreatedAt)
                .Select(m => m.ToDto())
                .ToList(),
            config.OutputFileNamePattern,
            detectedPlaceholders
        );

    internal static CertificateFieldMappingDto ToDto(this CertificateFieldMapping mapping) =>
        new(
            mapping.Id,
            mapping.FieldKey,
            mapping.SourceColumn,
            mapping.FieldType,
            mapping.PositionX,
            mapping.PositionY,
            mapping.Width,
            mapping.Height,
            mapping.FontSize,
            mapping.FontColor
        );

    internal static CertificateGenerationBatchDto ToDto(
        this CertificateGenerationBatch batch,
        IFileStorageService? storage = null) =>
        new(
            batch.Id,
            batch.CertificateTemplateConfigId,
            batch.Status,
            batch.OutputFormat,
            batch.TotalRecipients,
            batch.CompletedRecipients,
            batch.FailedRecipients,
            batch.Items
                .OrderBy(i => i.SequenceNumber)
                .Select(i => i.ToDto())
                .ToList(),
            OutputFileUrl: batch.OutputFilePath is not null && storage is not null
                ? storage.GetPublicUrl(batch.OutputFilePath)
                : null
        );

    internal static CertificateGenerationItemDto ToDto(this CertificateGenerationItem item) =>
        new(
            item.Id,
            item.SequenceNumber,
            item.RecipientName,
            item.RecipientEmail,
            item.Status
        );
}
