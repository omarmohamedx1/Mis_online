using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.Common;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/hr/audit")]
[Authorize(Policy = AuthorizationPolicies.HrDepartment)]
public sealed class HrAuditController : ControllerBase
{
    private readonly IHrAuditService _service;

    public HrAuditController(IHrAuditService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedAuditLogsDto>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
        {
            throw new HrValidationException("Page must be at least 1 and pageSize must be between 1 and 100.");
        }

        if (from.HasValue && to.HasValue && to < from)
        {
            throw new HrValidationException("The to date cannot be earlier than the from date.");
        }

        return Ok(await _service.GetPagedAsync(
            page,
            pageSize,
            search,
            action,
            entityType,
            employeeId,
            from,
            to,
            cancellationToken));
    }
}
