using MIS.Application.DTOs.Collections;

namespace MIS.Application.Interfaces;

public interface IBankArchiveService
{
    Task<ArchiveSummaryDto> SummaryAsync(Guid bankId, CancellationToken token);
    Task<ArchiveCasePageDto> CasesAsync(Guid bankId, ArchiveCaseQuery query, CancellationToken token);
    Task<ArchiveCaseDetailsDto> CaseAsync(Guid bankId, Guid caseId, CancellationToken token);
    Task ArchiveCaseAsync(Guid bankId, Guid caseId, ArchiveMutationRequest request, CancellationToken token);
    Task RestoreCaseAsync(Guid bankId, Guid caseId, RestoreMutationRequest request, CancellationToken token);
    Task<ArchivePortfolioPageDto> PortfoliosAsync(Guid bankId, ArchivePortfolioQuery query, CancellationToken token);
    Task ArchivePortfolioAsync(Guid bankId, Guid portfolioId, ArchiveMutationRequest request, CancellationToken token);
    Task RestorePortfolioAsync(Guid bankId, Guid portfolioId, RestoreMutationRequest request, CancellationToken token);
    Task<IReadOnlyCollection<ArchiveFilterOptionDto>> UsersAsync(Guid bankId, CancellationToken token);
}
