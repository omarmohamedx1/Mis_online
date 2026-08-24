using MIS.Application.DTOs.Collections;

namespace MIS.Application.Interfaces;

public interface IBankComplaintService
{
    Task<BankComplaintPageDto> GetAsync(Guid bankId, BankComplaintQuery query, CancellationToken token);
    Task<BankComplaintSummaryDto> SummaryAsync(Guid bankId, CancellationToken token);
    Task<BankComplaintDetailsDto> DetailsAsync(Guid bankId, Guid complaintId, CancellationToken token);
    Task<IReadOnlyCollection<BankComplaintCaseDto>> CasesAsync(Guid bankId, string? search, CancellationToken token);
    Task<IReadOnlyCollection<BankComplaintEmployeeDto>> EmployeesAsync(Guid bankId, CancellationToken token);
    Task<BankComplaintDetailsDto> CreateAsync(Guid bankId, CreateBankComplaintRequest request, CancellationToken token);
    Task<BankComplaintDetailsDto> AssignAsync(Guid bankId, Guid complaintId, AssignBankComplaintRequest request, CancellationToken token);
    Task<BankComplaintDetailsDto> ChangePriorityAsync(Guid bankId, Guid complaintId, ChangeBankComplaintPriorityRequest request, CancellationToken token);
    Task<BankComplaintDetailsDto> StartAsync(Guid bankId, Guid complaintId, ComplaintTransitionRequest request, CancellationToken token);
    Task<BankComplaintDetailsDto> AddNoteAsync(Guid bankId, Guid complaintId, AddBankComplaintNoteRequest request, CancellationToken token);
    Task<BankComplaintDetailsDto> ResolveAsync(Guid bankId, Guid complaintId, ResolveBankComplaintRequest request, CancellationToken token);
    Task<BankComplaintDetailsDto> CloseAsync(Guid bankId, Guid complaintId, ComplaintTransitionRequest request, CancellationToken token);
    Task<BankComplaintDetailsDto> ReopenAsync(Guid bankId, Guid complaintId, ComplaintReasonRequest request, CancellationToken token);
    Task<BankComplaintDetailsDto> RejectAsync(Guid bankId, Guid complaintId, ComplaintReasonRequest request, CancellationToken token);
    Task<BankComplaintDetailsDto> UploadAttachmentAsync(Guid bankId, Guid complaintId, string fileName, string contentType, long length, Stream content, CancellationToken token);
    Task<BankComplaintAttachmentDownloadDto> DownloadAttachmentAsync(Guid bankId, Guid complaintId, Guid attachmentId, CancellationToken token);
    Task RemoveAttachmentAsync(Guid bankId, Guid complaintId, Guid attachmentId, CancellationToken token);
}
