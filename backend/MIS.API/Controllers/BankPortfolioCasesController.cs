using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Collections;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/banks/{bankId:guid}/portfolio-cases")]
[Route("api/installment-companies/{bankId:guid}/portfolio-cases")]
[Authorize(Policy = AuthorizationPolicies.CollectionsAccess)]
public sealed class BankPortfolioCasesController(IBankPortfolioCaseService service) : ControllerBase
{
    [HttpGet]
    public Task<BankPortfolioCasePageDto> Get(Guid bankId, [FromQuery] BankPortfolioCaseQuery query, CancellationToken token) => service.GetAsync(bankId, query, token);
    [HttpGet("{caseId:guid}")]
    public Task<BankPortfolioCaseDetailsDto> GetCase(Guid bankId, Guid caseId, CancellationToken token) => service.GetCaseAsync(bankId, caseId, token);
    [HttpPatch("{caseId:guid}")]
    public Task<BankPortfolioCaseDetailsDto> Update(Guid bankId, Guid caseId, UpdateBankPortfolioCaseRequest request, CancellationToken token) => service.UpdateAsync(bankId, caseId, request, token);
    [HttpGet("collectors")]
    public Task<IReadOnlyCollection<BankPortfolioCollectorDto>> Collectors(Guid bankId, CancellationToken token) => service.GetCollectorsAsync(bankId, token);
    [HttpPost("assignment/preview")]
    public Task<BankPortfolioAssignmentPreviewDto> Preview(Guid bankId, AssignBankPortfolioCasesRequest request, CancellationToken token) => service.PreviewAssignmentAsync(bankId, request, token);
    [HttpPost("assignment")]
    public Task<BankPortfolioAssignmentPreviewDto> Assign(Guid bankId, AssignBankPortfolioCasesRequest request, CancellationToken token) => service.AssignAsync(bankId, request, token);
    [HttpGet("export.csv")]
    [Authorize(Policy = AuthorizationPolicies.CollectionsReportExport)]
    public async Task<IActionResult> Export(Guid bankId, [FromQuery] BankPortfolioCaseQuery query, CancellationToken token) =>
        File(await service.ExportCsvAsync(bankId, query, token), "text/csv; charset=utf-8", $"portfolio-{bankId:N}.csv");
}
