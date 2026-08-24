using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Collections;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController, Authorize(Policy = AuthorizationPolicies.CollectionsAccess), Route("api/banks/{bankId:guid}"), Route("api/installment-companies/{bankId:guid}")]
public sealed class BankArchiveController(IBankArchiveService service) : ControllerBase
{
    [HttpGet("archive/summary")] public Task<ArchiveSummaryDto> Summary(Guid bankId, CancellationToken token) => service.SummaryAsync(bankId, token);
    [HttpGet("archive/cases")] public Task<ArchiveCasePageDto> Cases(Guid bankId, [FromQuery] ArchiveCaseQuery query, CancellationToken token) => service.CasesAsync(bankId, query, token);
    [HttpGet("archive/cases/{caseId:guid}")] public Task<ArchiveCaseDetailsDto> Case(Guid bankId, Guid caseId, CancellationToken token) => service.CaseAsync(bankId, caseId, token);
    [HttpPost("cases/{caseId:guid}/archive")] public async Task<IActionResult> ArchiveCase(Guid bankId, Guid caseId, ArchiveMutationRequest request, CancellationToken token) { await service.ArchiveCaseAsync(bankId, caseId, request, token); return NoContent(); }
    [HttpPost("cases/{caseId:guid}/restore")] public async Task<IActionResult> RestoreCase(Guid bankId, Guid caseId, RestoreMutationRequest request, CancellationToken token) { await service.RestoreCaseAsync(bankId, caseId, request, token); return NoContent(); }
    [HttpGet("archive/portfolios")] public Task<ArchivePortfolioPageDto> Portfolios(Guid bankId, [FromQuery] ArchivePortfolioQuery query, CancellationToken token) => service.PortfoliosAsync(bankId, query, token);
    [HttpPost("portfolio-imports/{portfolioId:guid}/archive")] public async Task<IActionResult> ArchivePortfolio(Guid bankId, Guid portfolioId, ArchiveMutationRequest request, CancellationToken token) { await service.ArchivePortfolioAsync(bankId, portfolioId, request, token); return NoContent(); }
    [HttpPost("portfolio-imports/{portfolioId:guid}/restore")] public async Task<IActionResult> RestorePortfolio(Guid bankId, Guid portfolioId, RestoreMutationRequest request, CancellationToken token) { await service.RestorePortfolioAsync(bankId, portfolioId, request, token); return NoContent(); }
    [HttpGet("archive/users")] public Task<IReadOnlyCollection<ArchiveFilterOptionDto>> Users(Guid bankId, CancellationToken token) => service.UsersAsync(bankId, token);
}
