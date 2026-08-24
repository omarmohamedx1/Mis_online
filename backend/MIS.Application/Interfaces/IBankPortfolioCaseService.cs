using MIS.Application.DTOs.Collections;

namespace MIS.Application.Interfaces;

public interface IBankPortfolioCaseService
{
    Task<BankPortfolioCasePageDto> GetAsync(Guid bankId, BankPortfolioCaseQuery query, CancellationToken token);
    Task<BankPortfolioCaseDetailsDto> GetCaseAsync(Guid bankId, Guid caseId, CancellationToken token);
    Task<BankPortfolioCaseDetailsDto> UpdateAsync(Guid bankId, Guid caseId, UpdateBankPortfolioCaseRequest request, CancellationToken token);
    Task<IReadOnlyCollection<BankPortfolioCollectorDto>> GetCollectorsAsync(Guid bankId, CancellationToken token);
    Task<BankPortfolioAssignmentPreviewDto> PreviewAssignmentAsync(Guid bankId, AssignBankPortfolioCasesRequest request, CancellationToken token);
    Task<BankPortfolioAssignmentPreviewDto> AssignAsync(Guid bankId, AssignBankPortfolioCasesRequest request, CancellationToken token);
    Task<byte[]> ExportCsvAsync(Guid bankId, BankPortfolioCaseQuery query, CancellationToken token);
}
