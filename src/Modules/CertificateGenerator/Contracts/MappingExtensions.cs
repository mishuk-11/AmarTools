using AmarTools.BuildingBlocks.Interfaces;
using AmarTools.Domain.Entities;

namespace AmarTools.Modules.CertificateGenerator.Contracts;

internal static class MappingExtensions
{
    internal static CertificateTemplateSetupDto ToSetupDto(
        this CertificateTemplateConfig config,
        IFileStorageService storage) =>
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
            config.EmailSubject,
            config.EmailBody,
            config.FieldMappings
                .OrderBy(m => m.CreatedAt)
                .Select(m => m.ToDto())
                .ToList()
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

    internal static CertificateGenerationBatchDto ToDto(this CertificateGenerationBatch batch) =>
        new(
            batch.Id,
            batch.CertificateTemplateConfigId,
            batch.Status,
            batch.OutputFormat,
            batch.TotalRecipients,
            batch.Items
                .OrderBy(i => i.SequenceNumber)
                .Select(i => i.ToDto())
                .ToList()
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
