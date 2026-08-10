using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/hr")]
[Authorize(Policy = AuthorizationPolicies.HrDepartment)]
public sealed class HrController : ControllerBase
{
    private readonly IHrDashboardRepository _dashboardRepository;

    public HrController(IHrDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    [HttpGet("access")]
    public IActionResult VerifyAccess() => Ok(new { department = "HR" });

    [HttpGet("dashboard")]
    [ProducesResponseType<HrDashboardDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<HrDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        return Ok(await _dashboardRepository.GetDashboardAsync(cancellationToken));
    }

    [HttpGet("departments")]
    public async Task<ActionResult<IReadOnlyCollection<DepartmentOptionDto>>> GetDepartments(
        [FromServices] IHrEmployeeRepository employeeRepository,
        CancellationToken cancellationToken)
    {
        return Ok(await employeeRepository.GetDepartmentsAsync(cancellationToken));
    }
}
