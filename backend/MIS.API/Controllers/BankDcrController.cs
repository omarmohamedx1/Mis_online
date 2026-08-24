using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Collections;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/banks/{bankId:guid}/dcr")]
[Route("api/installment-companies/{bankId:guid}/dcr")]
[Authorize(Policy = AuthorizationPolicies.CollectionsAccess)]
public sealed class BankDcrController(IBankDcrService service) : ControllerBase
{
    [HttpGet] public Task<BankDcrPageDto> Get(Guid bankId, [FromQuery] BankDcrQuery query, CancellationToken token) => service.GetAsync(bankId, query, token);
    [HttpGet("cases")] public Task<IReadOnlyCollection<BankActivityCaseLookupDto>> Cases(Guid bankId, [FromQuery] string? search, CancellationToken token) => service.CasesAsync(bankId, search, token);
    [HttpGet("collectors")] public Task<IReadOnlyCollection<BankDcrCollectorDto>> Collectors(Guid bankId, CancellationToken token) => service.CollectorsAsync(bankId, token);
    [HttpGet("{dcrId:guid}")] public Task<BankDcrItemDto> Details(Guid bankId, Guid dcrId, CancellationToken token) => service.GetDetailsAsync(bankId, dcrId, token);
    [HttpPost] public async Task<ActionResult<BankDcrItemDto>> Create(Guid bankId, CreateBankDcrRequest request, CancellationToken token) { var result = await service.CreateAsync(bankId, request, token); return CreatedAtAction(nameof(Details), new { bankId, dcrId = result.Id }, result); }
}
