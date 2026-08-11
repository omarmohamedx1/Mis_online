using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/hr/employees/{employeeId:guid}")]
[Authorize(Policy = AuthorizationPolicies.HrDepartment)]
public sealed class HrEmployeeProfilesController : ControllerBase
{
    private readonly IHrEmployeeProfileService _service;

    public HrEmployeeProfilesController(IHrEmployeeProfileService service)
    {
        _service = service;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<EmployeeProfileDto>> GetProfile(Guid employeeId, CancellationToken cancellationToken)
        => Ok(await _service.GetProfileAsync(employeeId, cancellationToken));

    [HttpGet("reporting-line")]
    public async Task<ActionResult<EmployeeReportingLineDto>> GetReportingLine(Guid employeeId, CancellationToken cancellationToken)
        => Ok(await _service.GetReportingLineAsync(employeeId, cancellationToken));

    [HttpPut("personal")]
    public async Task<ActionResult<EmployeeProfileDto>> UpdatePersonal(
        Guid employeeId,
        UpdateEmployeePersonalRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.UpdatePersonalAsync(employeeId, request, cancellationToken));

    [HttpPut("contact")]
    public async Task<ActionResult<EmployeeProfileDto>> UpdateContact(
        Guid employeeId,
        UpdateEmployeeContactRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.UpdateContactAsync(employeeId, request, cancellationToken));

    [HttpPut("employment")]
    public async Task<ActionResult<EmployeeProfileDto>> UpdateEmployment(
        Guid employeeId,
        UpdateEmployeeEmploymentRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.UpdateEmploymentAsync(employeeId, request, cancellationToken));

    [HttpPut("contract")]
    public async Task<ActionResult<EmployeeProfileDto>> UpdateContract(
        Guid employeeId,
        UpdateEmployeeContractRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.UpdateContractAsync(employeeId, request, cancellationToken));

    [HttpPut("compensation")]
    public async Task<ActionResult<EmployeeProfileDto>> UpdateCompensation(
        Guid employeeId,
        UpdateEmployeeCompensationRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.UpdateCompensationAsync(employeeId, request, cancellationToken));

    [HttpPut("emergency-contact")]
    public async Task<ActionResult<EmployeeProfileDto>> UpdateEmergencyContact(
        Guid employeeId,
        UpdateEmployeeEmergencyContactRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.UpdateEmergencyContactAsync(employeeId, request, cancellationToken));

    [HttpPatch("status")]
    public async Task<ActionResult<EmployeeProfileDto>> ChangeStatus(
        Guid employeeId,
        ChangeEmployeeStatusRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.ChangeStatusAsync(employeeId, request, cancellationToken));
}
