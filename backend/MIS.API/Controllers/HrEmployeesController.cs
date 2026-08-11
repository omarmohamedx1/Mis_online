using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.Common;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;
using MIS.Domain.Entities;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/hr/employees")]
[Authorize(Policy = AuthorizationPolicies.HrDepartment)]
public sealed class HrEmployeesController : ControllerBase
{
    private readonly IHrEmployeeRepository _repository;
    private readonly IHrAuditService _audit;
    private readonly IHrTransactionRunner _transactions;

    public HrEmployeesController(
        IHrEmployeeRepository repository,
        IHrAuditService audit,
        IHrTransactionRunner transactions)
    {
        _repository = repository;
        _audit = audit;
        _transactions = transactions;
    }

    [HttpGet]
    public async Task<ActionResult<PagedEmployeesDto>> GetEmployees(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            return BadRequest(ApiErrorResponse.Failure("Page must be at least 1 and pageSize must be between 1 and 100."));
        if (search?.Length > 160)
            return BadRequest(ApiErrorResponse.Failure("Search cannot exceed 160 characters."));

        var normalizedStatus = status?.Trim().ToLowerInvariant();
        var employeeStatus = normalizedStatus switch
        {
            null or "" or "all" => null,
            "active" => Employee.ActiveStatus,
            "inactive" => Employee.InactiveStatus,
            "onleave" or "on_leave" or "on leave" => Employee.OnLeaveStatus,
            "suspended" => Employee.SuspendedStatus,
            "terminated" => Employee.TerminatedStatus,
            _ => null
        };
        if (!string.IsNullOrWhiteSpace(normalizedStatus) && normalizedStatus is not ("all" or "active" or "inactive" or "onleave" or "on_leave" or "on leave" or "suspended" or "terminated"))
            return BadRequest(ApiErrorResponse.Failure("Status must be all, active, inactive, on leave, suspended, or terminated."));

        return Ok(await _repository.GetPagedByStatusAsync(page, pageSize, search, departmentId, employeeStatus, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeDetailsDto>> GetEmployee(Guid id, CancellationToken cancellationToken)
    {
        var employee = await _repository.GetDetailsByIdAsync(id, cancellationToken);
        return employee is null ? NotFound(ApiErrorResponse.Failure("Employee was not found.")) : Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDetailsDto>> CreateEmployee(SaveEmployeeRequest request, CancellationToken cancellationToken)
    {
        var error = await ValidateRequestAsync(request, null, cancellationToken);
        if (error is not null) return error;

        var employee = new Employee(request.EmployeeNumber, request.FullName, request.DepartmentId, request.IsActive, DateTimeOffset.UtcNow);
        var created = await _transactions.ExecuteAsync(async token =>
        {
            _repository.Add(employee);
            await _repository.SaveChangesAsync(token);
            var details = await _repository.GetDetailsByIdAsync(employee.Id, token)
                ?? throw new InvalidOperationException("The created employee could not be reloaded.");
            await _audit.WriteAsync(new AuditWriteRequest(
                "EmployeeCreated",
                nameof(Employee),
                employee.Id.ToString(),
                employee.Id,
                null,
                details,
                $"Created employee {request.EmployeeNumber}."), token);
            return details;
        }, cancellationToken);
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmployeeDetailsDto>> UpdateEmployee(Guid id, SaveEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await _repository.GetTrackedByIdAsync(id, cancellationToken);
        if (employee is null) return NotFound(ApiErrorResponse.Failure("Employee was not found."));
        var oldValue = await _repository.GetDetailsByIdAsync(id, cancellationToken);
        var error = await ValidateRequestAsync(request, id, cancellationToken);
        if (error is not null) return error;

        employee.Update(request.EmployeeNumber, request.FullName, request.DepartmentId, request.IsActive, DateTimeOffset.UtcNow);
        var updated = await _transactions.ExecuteAsync(async token =>
        {
            await _repository.SaveChangesAsync(token);
            var details = await _repository.GetDetailsByIdAsync(id, token)
                ?? throw new InvalidOperationException("The updated employee could not be reloaded.");
            await _audit.WriteAsync(new AuditWriteRequest(
                "EmployeeUpdated",
                nameof(Employee),
                id.ToString(),
                id,
                oldValue,
                details,
                $"Updated employee {request.EmployeeNumber}."), token);
            return details;
        }, cancellationToken);
        return Ok(updated);
    }

    private async Task<ActionResult?> ValidateRequestAsync(SaveEmployeeRequest request, Guid? excludingId, CancellationToken cancellationToken)
    {
        if (request.DepartmentId == Guid.Empty || !await _repository.DepartmentExistsAsync(request.DepartmentId, cancellationToken))
            return BadRequest(ApiErrorResponse.Failure("A valid department is required."));
        if (await _repository.EmployeeNumberExistsAsync(request.EmployeeNumber, excludingId, cancellationToken))
            return Conflict(ApiErrorResponse.Failure("An employee with this employee ID already exists."));
        return null;
    }
}
