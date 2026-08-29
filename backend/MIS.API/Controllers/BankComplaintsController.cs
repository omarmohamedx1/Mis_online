using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIS.API.Authorization;
using MIS.Application.Common;
using MIS.Application.DTOs.Collections;
using MIS.Application.Interfaces;

namespace MIS.API.Controllers;

[ApiController]
[Route("api/banks/{bankId:guid}/complaints")]
[Route("api/installment-companies/{bankId:guid}/complaints")]
[Authorize(Policy = AuthorizationPolicies.CollectionsAccess)]
public sealed class BankComplaintsController(IBankComplaintService service) : ControllerBase
{
    [HttpGet] public Task<BankComplaintPageDto> Get(Guid bankId, [FromQuery] BankComplaintQuery query, CancellationToken token) => service.GetAsync(bankId, query, token);
    [HttpGet("summary")] public Task<BankComplaintSummaryDto> Summary(Guid bankId, CancellationToken token) => service.SummaryAsync(bankId, token);
    [HttpGet("cases")] public Task<IReadOnlyCollection<BankComplaintCaseDto>> Cases(Guid bankId, [FromQuery] string? search, CancellationToken token) => service.CasesAsync(bankId, search, token);
    [HttpGet("employees")] public Task<IReadOnlyCollection<BankComplaintEmployeeDto>> Employees(Guid bankId, CancellationToken token) => service.EmployeesAsync(bankId, token);
    [HttpGet("{complaintId:guid}")] public Task<BankComplaintDetailsDto> Details(Guid bankId, Guid complaintId, CancellationToken token) => service.DetailsAsync(bankId, complaintId, token);
    [HttpPost] public async Task<ActionResult<BankComplaintDetailsDto>> Create(Guid bankId, CreateBankComplaintRequest request, CancellationToken token) { var value = await service.CreateAsync(bankId, request, token); return Created($"/api/banks/{bankId}/complaints/{value.Id}", value); }
    [HttpPost("{complaintId:guid}/assign")] public Task<BankComplaintDetailsDto> Assign(Guid bankId, Guid complaintId, AssignBankComplaintRequest request, CancellationToken token) => service.AssignAsync(bankId, complaintId, request, token);
    [HttpPost("{complaintId:guid}/priority")] public Task<BankComplaintDetailsDto> Priority(Guid bankId, Guid complaintId, ChangeBankComplaintPriorityRequest request, CancellationToken token) => service.ChangePriorityAsync(bankId, complaintId, request, token);
    [HttpPost("{complaintId:guid}/start")] public Task<BankComplaintDetailsDto> Start(Guid bankId, Guid complaintId, ComplaintTransitionRequest request, CancellationToken token) => service.StartAsync(bankId, complaintId, request, token);
    [HttpPost("{complaintId:guid}/notes")] public Task<BankComplaintDetailsDto> Note(Guid bankId, Guid complaintId, AddBankComplaintNoteRequest request, CancellationToken token) => service.AddNoteAsync(bankId, complaintId, request, token);
    [HttpPost("{complaintId:guid}/resolve")] public Task<BankComplaintDetailsDto> Resolve(Guid bankId, Guid complaintId, ResolveBankComplaintRequest request, CancellationToken token) => service.ResolveAsync(bankId, complaintId, request, token);
    [HttpPost("{complaintId:guid}/close")] public Task<BankComplaintDetailsDto> Close(Guid bankId, Guid complaintId, ComplaintTransitionRequest request, CancellationToken token) => service.CloseAsync(bankId, complaintId, request, token);
    [HttpPost("{complaintId:guid}/reopen")] public Task<BankComplaintDetailsDto> Reopen(Guid bankId, Guid complaintId, ComplaintReasonRequest request, CancellationToken token) => service.ReopenAsync(bankId, complaintId, request, token);
    [HttpPost("{complaintId:guid}/reject")] public Task<BankComplaintDetailsDto> Reject(Guid bankId, Guid complaintId, ComplaintReasonRequest request, CancellationToken token) => service.RejectAsync(bankId, complaintId, request, token);
    [HttpPost("{complaintId:guid}/attachments")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<ActionResult<BankComplaintDetailsDto>> Upload(Guid bankId, Guid complaintId, IFormFile file, CancellationToken token) { if (file is null || file.Length == 0) return BadRequest(ApiErrorResponse.Failure("A non-empty attachment is required.")); await using var stream = file.OpenReadStream(); return await service.UploadAttachmentAsync(bankId, complaintId, file.FileName, file.ContentType ?? "application/octet-stream", file.Length, stream, token); }
    [HttpGet("{complaintId:guid}/attachments/{attachmentId:guid}")] public async Task<IActionResult> Download(Guid bankId, Guid complaintId, Guid attachmentId, CancellationToken token) { var value = await service.DownloadAttachmentAsync(bankId, complaintId, attachmentId, token); return File(value.Content, value.ContentType, value.FileName); }
    [HttpDelete("{complaintId:guid}/attachments/{attachmentId:guid}")] public async Task<IActionResult> Remove(Guid bankId, Guid complaintId, Guid attachmentId, CancellationToken token) { await service.RemoveAttachmentAsync(bankId, complaintId, attachmentId, token); return NoContent(); }
}
