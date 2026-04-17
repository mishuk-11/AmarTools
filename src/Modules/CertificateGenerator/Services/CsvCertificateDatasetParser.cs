using AmarTools.BuildingBlocks.Common;
using AmarTools.Modules.CertificateGenerator.Contracts;
using System.Text;

namespace AmarTools.Modules.CertificateGenerator.Services;

internal sealed class CsvCertificateDatasetParser : ICertificateDatasetParser
{
    private const int MaxPreviewRows = 5;

    public async Task<Result<CertificateDatasetPreviewDto>> ParsePreviewAsync(
        Stream datasetStream,
        string fileName,
        CancellationToken ct = default)
    {
        var parsedResult = await ParseRowsAsync(datasetStream, fileName, ct);
        if (parsedResult.IsFailure)
            return parsedResult.Error;

        var parsed = parsedResult.Value;
        var sampleRows = parsed.Rows.Take(MaxPreviewRows).ToList();

        return new CertificateDatasetPreviewDto(
            Path.GetFileName(fileName),
            parsed.Columns,
            sampleRows,
            sampleRows.Count);
    }

    public async Task<Result<CertificateParsedDatasetDto>> ParseRowsAsync(
        Stream datasetStream,
        string fileName,
        CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is not ".csv" and not ".txt")
            return Error.Validation("Certificates.UnsupportedDataset",
                "Only CSV files are supported for recipient dataset preview right now.");

        datasetStream.Position = 0;
        using var reader = new StreamReader(datasetStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(ct);
            if (!string.IsNullOrWhiteSpace(line))
                lines.Add(line);
        }

        if (lines.Count == 0)
            return Error.Validation("Certificates.EmptyDataset", "The uploaded dataset file is empty.");

        var headers = ParseCsvLine(lines[0]);
        if (headers.Count == 0 || headers.All(string.IsNullOrWhiteSpace))
            return Error.Validation("Certificates.InvalidDataset",
                "The uploaded dataset must contain a header row with at least one column.");

        var normalizedHeaders = headers
            .Select(h => h.Trim())
            .ToList();

        var rows = new List<IReadOnlyDictionary<string, string>>();

        foreach (var line in lines.Skip(1))
        {
            var cells = ParseCsvLine(line);
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < normalizedHeaders.Count; i++)
            {
                var key = normalizedHeaders[i];
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                row[key] = i < cells.Count ? cells[i].Trim() : string.Empty;
            }

            rows.Add(row);
        }

        return new CertificateParsedDatasetDto(normalizedHeaders, rows);
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        result.Add(current.ToString());
        return result;
    }
}
