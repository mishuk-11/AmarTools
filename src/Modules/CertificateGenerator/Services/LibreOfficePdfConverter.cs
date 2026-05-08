using System.Diagnostics;

namespace AmarTools.Modules.CertificateGenerator.Services;

internal sealed class LibreOfficePdfConverter : IPdfConverter
{
    private static readonly string? LibreOfficePath = FindLibreOffice();

    public bool IsAvailable => LibreOfficePath is not null;
    public string OutputExtension => "pdf";

    public async Task<MemoryStream> ConvertFromPptxAsync(Stream pptxStream, CancellationToken ct)
    {
        if (LibreOfficePath is null)
            throw new InvalidOperationException("LibreOffice is not available on this system.");

        var tempDir = Path.Combine(Path.GetTempPath(), $"cert_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var inputPath = Path.Combine(tempDir, "input.pptx");
            await using (var fs = File.Create(inputPath))
            {
                pptxStream.Position = 0;
                await pptxStream.CopyToAsync(fs, ct);
            }

            var psi = new ProcessStartInfo(LibreOfficePath)
            {
                Arguments = $"--headless --convert-to pdf --outdir \"{tempDir}\" \"{inputPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start LibreOffice process.");

            await process.WaitForExitAsync(ct);

            if (process.ExitCode != 0)
                throw new InvalidOperationException(
                    $"LibreOffice exited with code {process.ExitCode}.");

            var outputPath = Path.Combine(tempDir, "input.pdf");
            if (!File.Exists(outputPath))
                throw new InvalidOperationException("LibreOffice did not produce an output PDF.");

            var result = new MemoryStream();
            await using var outFs = File.OpenRead(outputPath);
            await outFs.CopyToAsync(result, ct);
            result.Position = 0;
            return result;
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { }
        }
    }

    private static string? FindLibreOffice()
    {
        var candidates = new[]
        {
            @"C:\Program Files\LibreOffice\program\soffice.exe",
            @"C:\Program Files (x86)\LibreOffice\program\soffice.exe",
            "/usr/bin/soffice",
            "/usr/bin/libreoffice",
            "/usr/local/bin/soffice",
            "soffice",
            "libreoffice"
        };

        foreach (var candidate in candidates)
        {
            try
            {
                var psi = new ProcessStartInfo(candidate)
                {
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(3000);
                if (p?.ExitCode == 0)
                    return candidate;
            }
            catch { }
        }

        return null;
    }
}
