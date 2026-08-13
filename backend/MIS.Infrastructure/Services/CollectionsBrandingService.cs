using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MIS.Application.Common;
using MIS.Application.Interfaces;
using MIS.Domain.Entities;
using MIS.Infrastructure.Persistence;

namespace MIS.Infrastructure.Services;

public sealed class CollectionsBrandingService : ICollectionsBrandingService
{
    private const long MaximumBytes = 2 * 1024 * 1024;
    private readonly ApplicationDbContext _db;
    private readonly IHrFileStorage _files;
    private readonly ICurrentUserContext _user;

    public CollectionsBrandingService(ApplicationDbContext db, IHrFileStorage files, ICurrentUserContext user)
    { _db = db; _files = files; _user = user; }

    public async Task<CollectionBrandLogoDto> UploadLogoAsync(Guid organizationId, string fileName, long length, Stream content, CancellationToken token)
    {
        if (length <= 0 || length > MaximumBytes) throw new HrValidationException("Client logos must be between 1 byte and 2 MB.");
        var entity = await _db.CollectionClientOrganizations.SingleOrDefaultAsync(x => x.Id == organizationId, token) ?? throw new HrNotFoundException("Client organization was not found.");
        var contentType = await ValidateImageAsync(content, Path.GetExtension(fileName).ToLowerInvariant(), token);
        var oldKey = entity.LogoStorageKey;
        var stored = await _files.SaveAsync("collections-branding", fileName, contentType, content, MaximumBytes, token);
        entity.SetLogo(stored.StorageKey, DateTimeOffset.UtcNow);
        _db.CollectionAuditLogs.Add(new CollectionAuditLog(_user.UserId, "ClientLogoUpdated", nameof(ClientOrganization), entity.Id, null, null, JsonSerializer.Serialize(new { entity.Code, stored.ContentType, stored.Length }), "WEB", DateTimeOffset.UtcNow));
        try { await _db.SaveChangesAsync(token); }
        catch { await _files.DeleteAsync(stored.StorageKey, token); throw; }
        if (!string.IsNullOrWhiteSpace(oldKey)) await _files.DeleteAsync(oldKey, token);
        return new CollectionBrandLogoDto(LogoUrl(entity.Id));
    }

    public async Task<CollectionBrandLogoDownloadDto> DownloadLogoAsync(Guid organizationId, CancellationToken token)
    {
        var key = await _db.CollectionClientOrganizations.AsNoTracking().Where(x => x.Id == organizationId).Select(x => x.LogoStorageKey).SingleOrDefaultAsync(token);
        if (string.IsNullOrWhiteSpace(key)) throw new HrNotFoundException("Client logo was not found.");
        var extension = Path.GetExtension(key).ToLowerInvariant();
        var contentType = extension switch { ".png" => "image/png", ".jpg" or ".jpeg" => "image/jpeg", ".webp" => "image/webp", _ => "application/octet-stream" };
        return new CollectionBrandLogoDownloadDto(await _files.OpenReadAsync(key, token), contentType);
    }

    public static string LogoUrl(Guid id) => $"/api/collections/branding/clients/{id}/logo";

    private static async Task<string> ValidateImageAsync(Stream stream, string extension, CancellationToken token)
    {
        if (!stream.CanSeek) throw new HrValidationException("Logo stream must be seekable.");
        var header = new byte[12]; var read = await stream.ReadAsync(header, token); stream.Position = 0;
        var png = read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var jpeg = read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        var webp = read >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8);
        return extension switch { ".png" when png => "image/png", ".jpg" or ".jpeg" when jpeg => "image/jpeg", ".webp" when webp => "image/webp", _ => throw new HrValidationException("Only content-validated PNG, JPEG, and WebP logos are accepted.") };
    }
}
