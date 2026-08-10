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
    public HrEmployeesController(IHrEmployeeRepository repository) => _repository = repository;

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
        bool? isActive = normalizedStatus switch
        {
            null or "" or "all" => null,
            "active" => true,
            "inactive" => false,
            _ => null
        };
        if (!string.IsNullOrWhiteSpace(normalizedStatus) && normalizedStatus is not ("all" or "active" or "inactive"))
            return BadRequest(ApiErrorResponse.Failure("Status must be all, active, or inactive."));

        return Ok(await _repository.GetPagedAsync(page, pageSize, search, departmentId, isActive, cancellationToken));
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
        _repository.Add(employee);
        await _repository.SaveChangesAsync(cancellationToken);
        var created = await _repository.GetDetailsByIdAsync(employee.Id, cancellationToken);
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmployeeDetailsDto>> UpdateEmployee(Guid id, SaveEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await _repository.GetTrackedByIdAsync(id, cancellationToken);
        if (employee is null) return NotFound(ApiErrorResponse.Failure("Employee was not found."));
        var error = await ValidateRequestAsync(request, id, cancellationToken);
        if (error is not null) return error;

        employee.Update(request.EmployeeNumber, request.FullName, request.DepartmentId, request.IsActive, DateTimeOffset.UtcNow);
        await _repository.SaveChangesAsync(cancellationToken);
        return Ok(await _repository.GetDetailsByIdAsync(id, cancellationToken));
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
