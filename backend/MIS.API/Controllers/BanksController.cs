using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Collections;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/banks")]
[Route("api/installment-companies")]
[Authorize(Policy = AuthorizationPolicies.CollectionsAccess)]
public sealed class BanksController : ControllerBase
{
    private readonly IBanksService _banks;

    public BanksController(IBanksService banks) => _banks = banks;

    [HttpGet]
    public Task<IReadOnlyCollection<BankDirectoryItemDto>> GetBanks(
        [FromQuery] string? search,
        CancellationToken token) => _banks.GetOrganizationsAsync(Request.Path.StartsWithSegments("/api/installment-companies") ? "CONSUMER_FINANCE" : "BANK", search, token);

    [HttpGet("{bankId:guid}")]
    public Task<BankDirectoryItemDto> GetBank(Guid bankId, CancellationToken token) =>
        _banks.GetOrganizationAsync(Request.Path.StartsWithSegments("/api/installment-companies") ? "CONSUMER_FINANCE" : "BANK", bankId, token);
}
