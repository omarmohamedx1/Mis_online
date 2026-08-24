using MIS.Application.DTOs.Collections;

namespace MIS.Application.Interfaces;

public interface IBankCaseDistributionService
{
    Task<CaseDistributionSummaryDto> SummaryAsync(Guid bankId, CancellationToken token);
    Task<CaseDistributionPageDto> CasesAsync(Guid bankId, bool assigned, CaseDistributionQuery query, CancellationToken token);
    Task<IReadOnlyCollection<DistributionCollectorDto>> CollectorsAsync(Guid bankId, CancellationToken token);
    Task<IReadOnlyCollection<DistributionImportDto>> ImportsAsync(Guid bankId, CancellationToken token);
    Task<DistributionPreviewDto> PreviewAsync(Guid bankId, bool reassign, DistributionMutationRequest request, CancellationToken token);
    Task<DistributionResultDto> AssignAsync(Guid bankId, bool reassign, DistributionMutationRequest request, CancellationToken token);
    Task<DistributionPreviewDto> PreviewUnassignAsync(Guid bankId, DistributionMutationRequest request, CancellationToken token);
    Task<DistributionResultDto> UnassignAsync(Guid bankId, DistributionMutationRequest request, CancellationToken token);
    Task<AutoDistributionPreviewDto> PreviewAutoAsync(Guid bankId, AutoDistributionRequest request, CancellationToken token);
    Task<DistributionResultDto> ConfirmAutoAsync(Guid bankId, AutoDistributionRequest request, CancellationToken token);
}
