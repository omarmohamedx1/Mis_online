using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Collections;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/banks/{bankId:guid}/activities")]
[Route("api/installment-companies/{bankId:guid}/activities")]
[Authorize(Policy = AuthorizationPolicies.CollectionsAccess)]
public sealed class BankCaseActivitiesController(IBankCaseActivityService service) : ControllerBase
{
    [HttpGet] public Task<BankCaseActivityPageDto> Get(Guid bankId, [FromQuery] BankCaseActivityQuery query, CancellationToken token) => service.GetAsync(bankId, query, token);
    [HttpGet("summary")] public Task<BankCaseActivitySummaryDto> Summary(Guid bankId, CancellationToken token) => service.SummaryAsync(bankId, token);
    [HttpGet("collectors")] public Task<IReadOnlyCollection<BankPortfolioCollectorDto>> Collectors(Guid bankId, CancellationToken token) => service.CollectorsAsync(bankId, token);
    [HttpGet("cases")] public Task<IReadOnlyCollection<BankActivityCaseLookupDto>> Cases(Guid bankId, [FromQuery] string? search, CancellationToken token) => service.CasesAsync(bankId, search, token);
    [HttpGet("{activityId:guid}")] public Task<BankCaseActivityDetailsDto> Details(Guid bankId, Guid activityId, CancellationToken token) => service.GetDetailsAsync(bankId, activityId, token);
    [HttpGet("case/{caseId:guid}")] public Task<IReadOnlyCollection<BankCaseActivityItemDto>> Timeline(Guid bankId, Guid caseId, CancellationToken token) => service.TimelineAsync(bankId, caseId, token);
    [HttpPost] public async Task<ActionResult<BankCaseActivityDetailsDto>> Create(Guid bankId, CreateBankCaseActivityRequest request, CancellationToken token)
    { var result = await service.CreateAsync(bankId, request, token); return CreatedAtAction(nameof(Details), new { bankId, activityId = result.Id }, result); }
}
