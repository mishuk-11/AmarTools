namespace AmarTools.Modules.CertificateGenerator.Contracts;

public sealed record CertificateBatchDownloadDto(
    Stream FileStream,
    string FileName,
    string ContentType
);
