using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/hr/calendar")]
[Authorize(Policy = AuthorizationPolicies.HrDepartment)]
public sealed class HrCalendarController : ControllerBase
{
    private readonly IHrCalendarService _service;

    public HrCalendarController(IHrCalendarService service)
    {
        _service = service;
    }

    [HttpGet("working-calendar")]
    public async Task<ActionResult<WorkingCalendarDto>> GetWorkingCalendar(CancellationToken cancellationToken)
        => Ok(await _service.GetWorkingCalendarAsync(cancellationToken));

    [HttpPut("working-calendar")]
    public async Task<ActionResult<WorkingCalendarDto>> UpdateWorkingCalendar(
        UpdateWorkingCalendarRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.UpdateWorkingCalendarAsync(request, cancellationToken));

    [HttpGet("exceptions")]
    public async Task<ActionResult<PagedCalendarExceptionsDto>> GetExceptions(
        [FromQuery] CalendarExceptionFilterDto filter,
        CancellationToken cancellationToken)
        => Ok(await _service.GetExceptionsAsync(filter, cancellationToken));

    [HttpGet("exceptions/{id:guid}")]
    public async Task<ActionResult<CalendarExceptionDetailsDto>> GetException(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetExceptionAsync(id, cancellationToken));

    [HttpPost("exceptions")]
    public async Task<ActionResult<CalendarExceptionDetailsDto>> CreateException(
        CreateCalendarExceptionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateExceptionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetException), new { id = created.Id }, created);
    }

    [HttpPut("exceptions/{id:guid}")]
    public async Task<ActionResult<CalendarExceptionDetailsDto>> UpdateException(
        Guid id,
        UpdateCalendarExceptionRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.UpdateExceptionAsync(id, request, cancellationToken));

    [HttpPatch("exceptions/{id:guid}/active")]
    public async Task<ActionResult<CalendarExceptionDetailsDto>> SetExceptionActive(
        Guid id,
        SetCalendarExceptionActiveRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.SetExceptionActiveAsync(id, request.IsActive, cancellationToken));

    [HttpDelete("exceptions/{id:guid}")]
    public async Task<IActionResult> DeleteException(
        Guid id,
        [FromBody] DeleteCalendarExceptionRequest request,
        CancellationToken cancellationToken)
    {
        await _service.DeleteExceptionAsync(id, request, cancellationToken);
        return NoContent();
    }
}
