using MIS.Application.DTOs.Collections;

namespace MIS.Application.Interfaces;

public interface IBankPtpService
{
    Task<BankPtpSummaryDto> SummaryAsync(Guid bankId, CancellationToken token);
    Task<BankPtpPageDto> GetAsync(Guid bankId, BankPtpQuery query, CancellationToken token);
    Task<BankPtpDetailsDto> GetDetailsAsync(Guid bankId, Guid ptpId, CancellationToken token);
    Task<BankPtpDetailsDto> CreateAsync(Guid bankId, CreateBankPtpRequest request, CancellationToken token);
    Task<BankPtpDetailsDto> ChangeStatusAsync(Guid bankId, Guid ptpId, ChangeBankPtpStatusRequest request, CancellationToken token);
    Task<IReadOnlyCollection<BankActivityCaseLookupDto>> CasesAsync(Guid bankId, string? search, CancellationToken token);
    Task<IReadOnlyCollection<BankPortfolioCollectorDto>> CollectorsAsync(Guid bankId, CancellationToken token);
}
