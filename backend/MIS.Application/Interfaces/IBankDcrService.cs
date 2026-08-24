using MIS.Application.DTOs.Collections;

namespace MIS.Application.Interfaces;

public interface IBankDcrService
{
    Task<BankDcrPageDto> GetAsync(Guid bankId, BankDcrQuery query, CancellationToken token);
    Task<BankDcrItemDto> GetDetailsAsync(Guid bankId, Guid dcrId, CancellationToken token);
    Task<BankDcrItemDto> CreateAsync(Guid bankId, CreateBankDcrRequest request, CancellationToken token);
    Task<IReadOnlyCollection<BankActivityCaseLookupDto>> CasesAsync(Guid bankId, string? search, CancellationToken token);
    Task<IReadOnlyCollection<BankDcrCollectorDto>> CollectorsAsync(Guid bankId, CancellationToken token);
}
