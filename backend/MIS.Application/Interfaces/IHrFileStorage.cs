using MIS.Application.DTOs.Hr;

namespace MIS.Application.Interfaces;

public interface IHrFileStorage
{
    Task<StoredFileDto> SaveAsync(
        string scope,
        string originalFileName,
        string contentType,
        Stream content,
        long maximumBytes,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken);
}
