using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Collections;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/banks/{bankId:guid}/distribution")]
[Route("api/installment-companies/{bankId:guid}/distribution")]
[Authorize(Policy = AuthorizationPolicies.CollectionsAssignmentManage)]
public sealed class BankCaseDistributionController(IBankCaseDistributionService service) : ControllerBase
{
    [HttpGet("summary")] public Task<CaseDistributionSummaryDto> Summary(Guid bankId, CancellationToken token) => service.SummaryAsync(bankId, token);
    [HttpGet("unassigned")] public Task<CaseDistributionPageDto> Unassigned(Guid bankId, [FromQuery] CaseDistributionQuery query, CancellationToken token) => service.CasesAsync(bankId, false, query, token);
    [HttpGet("assigned")] public Task<CaseDistributionPageDto> Assigned(Guid bankId, [FromQuery] CaseDistributionQuery query, CancellationToken token) => service.CasesAsync(bankId, true, query, token);
    [HttpGet("collectors")] public Task<IReadOnlyCollection<DistributionCollectorDto>> Collectors(Guid bankId, CancellationToken token) => service.CollectorsAsync(bankId, token);
    [HttpGet("imports")] public Task<IReadOnlyCollection<DistributionImportDto>> Imports(Guid bankId, CancellationToken token) => service.ImportsAsync(bankId, token);
    [HttpPost("assign/preview")] public Task<DistributionPreviewDto> PreviewAssign(Guid bankId, DistributionMutationRequest request, CancellationToken token) => service.PreviewAsync(bankId, false, request, token);
    [HttpPost("assign")] public Task<DistributionResultDto> Assign(Guid bankId, DistributionMutationRequest request, CancellationToken token) => service.AssignAsync(bankId, false, request, token);
    [HttpPost("reassign/preview")] public Task<DistributionPreviewDto> PreviewReassign(Guid bankId, DistributionMutationRequest request, CancellationToken token) => service.PreviewAsync(bankId, true, request, token);
    [HttpPost("reassign")] public Task<DistributionResultDto> Reassign(Guid bankId, DistributionMutationRequest request, CancellationToken token) => service.AssignAsync(bankId, true, request, token);
    [HttpPost("unassign/preview")] public Task<DistributionPreviewDto> PreviewUnassign(Guid bankId, DistributionMutationRequest request, CancellationToken token) => service.PreviewUnassignAsync(bankId, request, token);
    [HttpPost("unassign")] public Task<DistributionResultDto> Unassign(Guid bankId, DistributionMutationRequest request, CancellationToken token) => service.UnassignAsync(bankId, request, token);
    [HttpPost("auto/preview")] public Task<AutoDistributionPreviewDto> PreviewAuto(Guid bankId, AutoDistributionRequest request, CancellationToken token) => service.PreviewAutoAsync(bankId, request, token);
    [HttpPost("auto/confirm")] public Task<DistributionResultDto> ConfirmAuto(Guid bankId, AutoDistributionRequest request, CancellationToken token) => service.ConfirmAutoAsync(bankId, request, token);
}
