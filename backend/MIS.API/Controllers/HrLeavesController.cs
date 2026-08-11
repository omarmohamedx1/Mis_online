using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/hr/leaves")]
[Authorize(Policy = AuthorizationPolicies.HrDepartment)]
public sealed class HrLeavesController : ControllerBase
{
    private readonly IHrLeaveService _service;

    public HrLeavesController(IHrLeaveService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedLeaveRequestsDto>> GetPaged(
        [FromQuery] LeaveRequestFilterDto filter,
        CancellationToken cancellationToken)
        => Ok(await _service.GetPagedAsync(filter, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LeaveRequestDetailsDto>> GetDetails(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetDetailsAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<LeaveRequestDetailsDto>> Create(
        CreateLeaveRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetDetails), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LeaveRequestDetailsDto>> Update(
        Guid id,
        UpdateLeaveRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<LeaveRequestDetailsDto>> Approve(
        Guid id,
        ApproveLeaveRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.ApproveAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<LeaveRequestDetailsDto>> Reject(
        Guid id,
        RejectLeaveRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.RejectAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<LeaveRequestDetailsDto>> Cancel(
        Guid id,
        CancelLeaveRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.CancelAsync(id, request, cancellationToken));

    [HttpGet("balances")]
    public async Task<ActionResult<PagedLeaveBalancesDto>> GetBalances(
        [FromQuery] LeaveBalanceFilterDto filter,
        CancellationToken cancellationToken)
        => Ok(await _service.GetBalancesAsync(filter, cancellationToken));

    [HttpGet("employees/{employeeId:guid}/balances")]
    public async Task<ActionResult<IReadOnlyCollection<LeaveBalanceDto>>> GetEmployeeBalances(
        Guid employeeId,
        [FromQuery] int year,
        CancellationToken cancellationToken)
        => Ok(await _service.GetEmployeeBalancesAsync(employeeId, year == 0 ? DateTime.UtcNow.Year : year, cancellationToken));

    [HttpPut("employees/{employeeId:guid}/entitlements/{leaveTypeId:guid}/{year:int}")]
    public async Task<ActionResult<LeaveEntitlementDto>> UpsertEntitlement(
        Guid employeeId,
        Guid leaveTypeId,
        int year,
        UpsertLeaveEntitlementRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.UpsertEntitlementAsync(employeeId, leaveTypeId, year, request, cancellationToken));
}
