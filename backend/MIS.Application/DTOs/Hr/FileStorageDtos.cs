namespace MIS.Application.DTOs.Hr;

public sealed record StoredFileDto(
    string StorageKey,
    string OriginalFileName,
    string ContentType,
    long Length,
    string Sha256Hash);
