using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/hr/attendance")]
[Authorize(Policy = AuthorizationPolicies.HrDepartment)]
public sealed class HrAttendanceController : ControllerBase
{
    private readonly IHrAttendanceService _service;

    public HrAttendanceController(IHrAttendanceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedAttendanceRecordsDto>> GetPaged(
        [FromQuery] AttendanceFilterDto filter,
        CancellationToken cancellationToken)
        => Ok(await _service.GetPagedAsync(filter, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AttendanceDetailsDto>> GetDetails(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetDetailsAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<AttendanceDetailsDto>> CreateManual(
        CreateManualAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateManualAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetDetails), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AttendanceDetailsDto>> UpdateManual(
        Guid id,
        UpdateManualAttendanceRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.UpdateManualAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromBody] DeleteAttendanceRequest? request,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, request ?? new DeleteAttendanceRequest(), cancellationToken);
        return NoContent();
    }

    [HttpPost("process-day")]
    public async Task<ActionResult<ProcessAttendanceDayResultDto>> ProcessDay(
        ProcessAttendanceDayRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.ProcessDayAsync(request, cancellationToken));
}
