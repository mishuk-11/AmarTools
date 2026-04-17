using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.CertificateGenerator.Contracts;

namespace AmarTools.Modules.CertificateGenerator.Services;

/// <summary>
/// Parses uploaded recipient datasets and extracts columns plus sample rows for preview.
/// </summary>
public interface ICertificateDatasetParser
{
    Task<Result<CertificateDatasetPreviewDto>> ParsePreviewAsync(
        Stream datasetStream,
        string fileName,
        CancellationToken ct = default);

    Task<Result<CertificateParsedDatasetDto>> ParseRowsAsync(
        Stream datasetStream,
        string fileName,
        CancellationToken ct = default);
}
