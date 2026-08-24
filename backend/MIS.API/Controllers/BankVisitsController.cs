using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Collections;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/banks/{bankId:guid}/visits")]
[Route("api/installment-companies/{bankId:guid}/visits")]
[Authorize(Policy = AuthorizationPolicies.CollectionsAccess)]
public sealed class BankVisitsController(IBankVisitService service) : ControllerBase
{
    [HttpGet] public Task<BankVisitPageDto> Get(Guid bankId, [FromQuery] BankVisitQuery query, CancellationToken token) => service.GetAsync(bankId, query, token);
    [HttpGet("summary")] public Task<BankVisitSummaryDto> Summary(Guid bankId, CancellationToken token) => service.SummaryAsync(bankId, token);
    [HttpGet("cases")] public Task<IReadOnlyCollection<BankVisitCaseLookupDto>> Cases(Guid bankId, [FromQuery] string? search, CancellationToken token) => service.CasesAsync(bankId, search, token);
    [HttpGet("collectors")] public Task<IReadOnlyCollection<BankPortfolioCollectorDto>> Collectors(Guid bankId, CancellationToken token) => service.CollectorsAsync(bankId, token);
    [HttpGet("{visitId:guid}")] public Task<BankVisitDetailsDto> Details(Guid bankId, Guid visitId, CancellationToken token) => service.GetDetailsAsync(bankId, visitId, token);
    [HttpPost] public async Task<ActionResult<BankVisitDetailsDto>> Create(Guid bankId, CreateBankVisitRequest request, CancellationToken token) { var result = await service.CreateAsync(bankId, request, token); return CreatedAtAction(nameof(Details), new { bankId, visitId = result.Id }, result); }
    [HttpPost("{visitId:guid}/complete")] public Task<BankVisitDetailsDto> Complete(Guid bankId, Guid visitId, CompleteBankVisitRequest request, CancellationToken token) => service.CompleteAsync(bankId, visitId, request, token);
    [HttpPost("{visitId:guid}/reschedule")] public Task<BankVisitDetailsDto> Reschedule(Guid bankId, Guid visitId, RescheduleBankVisitRequest request, CancellationToken token) => service.RescheduleAsync(bankId, visitId, request, token);
    [HttpPost("{visitId:guid}/reassign")] public Task<BankVisitDetailsDto> Reassign(Guid bankId, Guid visitId, ReassignBankVisitRequest request, CancellationToken token) => service.ReassignAsync(bankId, visitId, request, token);
    [HttpPost("{visitId:guid}/cancel")] public Task<BankVisitDetailsDto> Cancel(Guid bankId, Guid visitId, CancelBankVisitRequest request, CancellationToken token) => service.CancelAsync(bankId, visitId, request, token);
    [HttpPost("{visitId:guid}/status")] public Task<BankVisitDetailsDto> ChangeStatus(Guid bankId, Guid visitId, ChangeBankVisitStatusRequest request, CancellationToken token) => service.ChangeStatusAsync(bankId, visitId, request, token);
}
