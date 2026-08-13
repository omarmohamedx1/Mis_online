namespace MIS.Application.Interfaces;

public sealed record CollectionBrandLogoDto(string LogoUrl);
public sealed record CollectionBrandLogoDownloadDto(Stream Content, string ContentType);

public interface ICollectionsBrandingService
{
    Task<CollectionBrandLogoDto> UploadLogoAsync(Guid organizationId, string fileName, long length, Stream content, CancellationToken cancellationToken);
    Task<CollectionBrandLogoDownloadDto> DownloadLogoAsync(Guid organizationId, CancellationToken cancellationToken);
}
