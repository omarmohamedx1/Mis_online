using MIS.Application.DTOs.Hr;

namespace MIS.Application.Interfaces;

public interface IHrDelegationService
{
    Task<PagedDelegationsDto> GetPagedAsync(DelegationFilterDto filter, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DelegationEntityOptionDto>> GetEntitiesAsync(CancellationToken cancellationToken);
    Task<DelegationDetailsDto> GetDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<DelegationDetailsDto> CreateAsync(CreateDelegationRequest request, CancellationToken cancellationToken);
    Task<DelegationDetailsDto> UpdateAsync(Guid id, UpdateDelegationRequest request, CancellationToken cancellationToken);
    Task<DelegationDetailsDto> CancelAsync(Guid id, CancelDelegationRequest request, CancellationToken cancellationToken);
    Task<DelegationPrintDto> GetPrintAsync(Guid id, CancellationToken cancellationToken);
}
