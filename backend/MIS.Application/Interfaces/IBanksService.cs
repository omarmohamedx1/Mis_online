using MIS.Application.DTOs.Collections;

namespace MIS.Application.Interfaces;

public interface IBanksService
{
    Task<IReadOnlyCollection<BankDirectoryItemDto>> GetOrganizationsAsync(string organizationType, string? search, CancellationToken cancellationToken);
    Task<BankDirectoryItemDto> GetOrganizationAsync(string organizationType, Guid organizationId, CancellationToken cancellationToken);
}
