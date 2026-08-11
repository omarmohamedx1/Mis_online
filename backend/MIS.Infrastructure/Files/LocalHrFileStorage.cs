using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using MIS.Application.Common;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;

namespace MIS.Infrastructure.Files;

public sealed class LocalHrFileStorage : IHrFileStorage
{
    private readonly string _rootPath;

    public LocalHrFileStorage(IOptions<HrFileStorageOptions> options)
    {
        var configuredPath = options.Value.RootPath;
        _rootPath = Path.GetFullPath(Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(AppContext.BaseDirectory, configuredPath));
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<StoredFileDto> SaveAsync(
        string scope,
        string originalFileName,
        string contentType,
        Stream content,
        long maximumBytes,
        CancellationToken cancellationToken)
    {
        if (maximumBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maximumBytes));
        if (string.IsNullOrWhiteSpace(originalFileName)) throw new HrValidationException("A file name is required.");

        var safeScope = NormalizeScope(scope);
        var extension = NormalizeExtension(Path.GetExtension(originalFileName));
        var datePath = DateTime.UtcNow.ToString("yyyy/MM");
        var storageKey = $"{safeScope}/{datePath}/{Guid.NewGuid():N}{extension}";
        var absolutePath = ResolvePath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        long totalBytes = 0;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        try
        {
            await using var output = new FileStream(
                absolutePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var buffer = new byte[81920];
            int read;
            while ((read = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                totalBytes += read;
                if (totalBytes > maximumBytes)
                    throw new HrValidationException($"The uploaded file exceeds the {maximumBytes / 1024 / 1024} MB limit.");

                hash.AppendData(buffer, 0, read);
                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
        }
        catch
        {
            if (File.Exists(absolutePath)) File.Delete(absolutePath);
            throw;
        }

        if (totalBytes == 0)
        {
            File.Delete(absolutePath);
            throw new HrValidationException("The uploaded file is empty.");
        }

        return new StoredFileDto(
            storageKey,
            Path.GetFileName(originalFileName),
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            totalBytes,
            Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolvePath(storageKey);
        if (!File.Exists(path)) throw new HrNotFoundException("The stored file was not found.");
        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolvePath(storageKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(ResolvePath(storageKey)));
    }

    private string ResolvePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey)) throw new HrValidationException("A storage key is required.");
        var normalized = storageKey.Replace('/', Path.DirectorySeparatorChar);
        var resolved = Path.GetFullPath(Path.Combine(_rootPath, normalized));
        var requiredPrefix = _rootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!resolved.StartsWith(requiredPrefix, StringComparison.OrdinalIgnoreCase))
            throw new HrValidationException("The storage key is invalid.");
        return resolved;
    }

    private static string NormalizeScope(string scope)
    {
        if (string.IsNullOrWhiteSpace(scope)) return "general";
        var normalized = new string(scope.Trim().ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '-')
            .ToArray()).Trim('-');
        return string.IsNullOrWhiteSpace(normalized) ? "general" : normalized;
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return string.Empty;
        var normalized = extension.ToLowerInvariant();
        if (normalized.Length > 12 || normalized.Skip(1).Any(character => !char.IsLetterOrDigit(character)))
            throw new HrValidationException("The file extension is invalid.");
        return normalized;
    }
}
