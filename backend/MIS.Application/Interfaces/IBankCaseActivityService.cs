using MIS.Application.DTOs.Collections;

namespace MIS.Application.Interfaces;

public interface IBankCaseActivityService
{
    Task<BankCaseActivitySummaryDto> SummaryAsync(Guid bankId, CancellationToken token);
    Task<BankCaseActivityPageDto> GetAsync(Guid bankId, BankCaseActivityQuery query, CancellationToken token);
    Task<BankCaseActivityDetailsDto> GetDetailsAsync(Guid bankId, Guid activityId, CancellationToken token);
    Task<IReadOnlyCollection<BankCaseActivityItemDto>> TimelineAsync(Guid bankId, Guid caseId, CancellationToken token);
    Task<IReadOnlyCollection<BankActivityCaseLookupDto>> CasesAsync(Guid bankId, string? search, CancellationToken token);
    Task<IReadOnlyCollection<BankPortfolioCollectorDto>> CollectorsAsync(Guid bankId, CancellationToken token);
    Task<BankCaseActivityDetailsDto> CreateAsync(Guid bankId, CreateBankCaseActivityRequest request, CancellationToken token);
}
