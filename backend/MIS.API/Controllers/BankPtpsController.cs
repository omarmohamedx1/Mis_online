using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Collections;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/banks/{bankId:guid}/ptps")]
[Route("api/installment-companies/{bankId:guid}/ptps")]
[Authorize(Policy = AuthorizationPolicies.CollectionsAccess)]
public sealed class BankPtpsController(IBankPtpService service) : ControllerBase
{
    [HttpGet] public Task<BankPtpPageDto> Get(Guid bankId,[FromQuery] BankPtpQuery query,CancellationToken token)=>service.GetAsync(bankId,query,token);
    [HttpGet("summary")] public Task<BankPtpSummaryDto> Summary(Guid bankId,CancellationToken token)=>service.SummaryAsync(bankId,token);
    [HttpGet("cases")] public Task<IReadOnlyCollection<BankActivityCaseLookupDto>> Cases(Guid bankId,[FromQuery]string? search,CancellationToken token)=>service.CasesAsync(bankId,search,token);
    [HttpGet("collectors")] public Task<IReadOnlyCollection<BankPortfolioCollectorDto>> Collectors(Guid bankId,CancellationToken token)=>service.CollectorsAsync(bankId,token);
    [HttpGet("{ptpId:guid}")] public Task<BankPtpDetailsDto> Details(Guid bankId,Guid ptpId,CancellationToken token)=>service.GetDetailsAsync(bankId,ptpId,token);
    [HttpPost] public async Task<ActionResult<BankPtpDetailsDto>> Create(Guid bankId,CreateBankPtpRequest request,CancellationToken token){var result=await service.CreateAsync(bankId,request,token);return CreatedAtAction(nameof(Details),new{bankId,ptpId=result.Id},result);}
    [HttpPost("{ptpId:guid}/status")] public Task<BankPtpDetailsDto> ChangeStatus(Guid bankId,Guid ptpId,ChangeBankPtpStatusRequest request,CancellationToken token)=>service.ChangeStatusAsync(bankId,ptpId,request,token);
}
