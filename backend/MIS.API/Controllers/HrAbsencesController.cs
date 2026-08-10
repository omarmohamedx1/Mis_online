using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.Common;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;
using MIS.Domain.Constants;
using MIS.Domain.Entities;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/hr/absences")]
[Authorize(Policy = AuthorizationPolicies.HrDepartment)]
public sealed class HrAbsencesController : ControllerBase
{
    private readonly IHrAbsenceRepository _repository;
    public HrAbsencesController(IHrAbsenceRepository repository) => _repository = repository;

    [HttpGet]
    public async Task<ActionResult<PagedAbsencesDto>> GetAbsences([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] Guid? departmentId = null, [FromQuery] DateOnly? date = null, [FromQuery] string? status = null, CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100) return BadRequest(ApiErrorResponse.Failure("Page must be at least 1 and pageSize must be between 1 and 100."));
        if (search?.Length > 160) return BadRequest(ApiErrorResponse.Failure("Search cannot exceed 160 characters."));
        var normalizedStatus = string.IsNullOrWhiteSpace(status) || status.Equals("all", StringComparison.OrdinalIgnoreCase) ? null : NormalizeStatus(status);
        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase) && normalizedStatus is null) return BadRequest(ApiErrorResponse.Failure("Status must be all, pending, excused, or unexcused."));
        return Ok(await _repository.GetPagedAsync(page, pageSize, search, departmentId, date, normalizedStatus, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AbsenceDetailsDto>> GetAbsence(Guid id, CancellationToken cancellationToken)
    {
        var absence = await _repository.GetDetailsAsync(id, cancellationToken);
        return absence is null ? NotFound(ApiErrorResponse.Failure("Absence record was not found.")) : Ok(absence);
    }

    [HttpPost]
    public async Task<ActionResult<AbsenceDetailsDto>> CreateAbsence(SaveAbsenceRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateAsync(request, cancellationToken); if (validation is not null) return validation;
        var absence = new EmployeeAbsence(request.EmployeeId, request.AbsenceDate, request.Reason, NormalizeStatus(request.Status)!, request.Notes, DateTimeOffset.UtcNow);
        _repository.Add(absence); await _repository.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetAbsence), new { id = absence.Id }, await _repository.GetDetailsAsync(absence.Id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AbsenceDetailsDto>> UpdateAbsence(Guid id, SaveAbsenceRequest request, CancellationToken cancellationToken)
    {
        var absence = await _repository.GetTrackedAsync(id, cancellationToken);
        if (absence is null) return NotFound(ApiErrorResponse.Failure("Absence record was not found."));
        var validation = await ValidateAsync(request, cancellationToken); if (validation is not null) return validation;
        absence.Update(request.EmployeeId, request.AbsenceDate, request.Reason, NormalizeStatus(request.Status)!, request.Notes, DateTimeOffset.UtcNow);
        await _repository.SaveChangesAsync(cancellationToken);
        return Ok(await _repository.GetDetailsAsync(id, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAbsence(Guid id, CancellationToken cancellationToken)
    {
        var absence = await _repository.GetTrackedAsync(id, cancellationToken);
        if (absence is null) return NotFound(ApiErrorResponse.Failure("Absence record was not found."));
        _repository.Remove(absence); await _repository.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<ActionResult?> ValidateAsync(SaveAbsenceRequest request, CancellationToken cancellationToken)
    {
        if (request.EmployeeId == Guid.Empty || !await _repository.EmployeeExistsAsync(request.EmployeeId, cancellationToken)) return BadRequest(ApiErrorResponse.Failure("A valid employee is required."));
        if (request.AbsenceDate == default) return BadRequest(ApiErrorResponse.Failure("A valid absence date is required."));
        if (!string.Equals(request.Type, AbsenceValues.AbsentType, StringComparison.OrdinalIgnoreCase)) return BadRequest(ApiErrorResponse.Failure("Type must be Absent for V1."));
        if (!string.Equals(request.AttendanceSource, AbsenceValues.ManualSource, StringComparison.OrdinalIgnoreCase)) return BadRequest(ApiErrorResponse.Failure("Attendance source must be Manual for V1."));
        if (NormalizeStatus(request.Status) is null) return BadRequest(ApiErrorResponse.Failure("Status must be Pending, Excused, or Unexcused."));
        return null;
    }

    private static string? NormalizeStatus(string? value) => value?.Trim().ToLowerInvariant() switch { "pending" => AbsenceValues.PendingStatus, "excused" => AbsenceValues.ExcusedStatus, "unexcused" => AbsenceValues.UnexcusedStatus, _ => null };
}
