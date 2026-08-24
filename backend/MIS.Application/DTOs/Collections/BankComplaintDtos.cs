using System.ComponentModel.DataAnnotations;

namespace MIS.Application.DTOs.Collections;

public sealed record BankComplaintQuery(int Page = 1, int PageSize = 20, string? View = null, string? Search = null, string? Status = null, string? Priority = null, string? Type = null, Guid? AssignedToId = null, DateOnly? CreatedDate = null, DateOnly? DueDate = null, string? SortBy = null, string? SortDirection = null);
public sealed record BankComplaintSummaryDto(int Open, int InProgress, int HighCritical, int Overdue, int ResolvedToday);
public sealed record BankComplaintAccessDto(bool IsManager, bool CanCreate, bool CanAssign, bool CanManageWorkflow, bool CanAddNote, bool CanUploadAttachment);
public sealed record BankComplaintItemDto(Guid Id, string ComplaintNumber, Guid CaseId, string CaseNumber, string CustomerName, string Type, string Priority, string Status, Guid? AssignedToId, string? AssignedToName, DateTimeOffset CreatedAt, DateTimeOffset? DueAt, DateTimeOffset UpdatedAt, bool IsOverdue);
public sealed record BankComplaintPageDto(IReadOnlyCollection<BankComplaintItemDto> Items, int TotalCount, int Page, int PageSize, int TotalPages, BankComplaintAccessDto Access);
public sealed record BankComplaintCaseDto(Guid Id, string CaseNumber, string CustomerName, string? Mobile);
public sealed record BankComplaintEmployeeDto(Guid Id, string Name);
public sealed record BankComplaintNoteDto(Guid Id, string Text, Guid CreatedById, string CreatedByName, DateTimeOffset CreatedAt);
public sealed record BankComplaintAttachmentDto(Guid Id, string FileName, string ContentType, long FileSize, Guid UploadedById, string UploadedByName, DateTimeOffset UploadedAt);
public sealed record BankComplaintHistoryDto(Guid Id, string Action, string? BeforeJson, string? AfterJson, Guid? UserId, string UserName, DateTimeOffset OccurredAt);
public sealed record BankComplaintDetailsDto(Guid Id, string ComplaintNumber, Guid CaseId, string CaseNumber, string CustomerName, string? Mobile, string BankName, string Type, string Priority, string Status, string Description, Guid? AssignedToId, string? AssignedToName, Guid CreatedById, string CreatedByName, DateTimeOffset CreatedAt, DateTimeOffset? DueAt, DateTimeOffset UpdatedAt, string? Resolution, Guid? ResolvedById, string? ResolvedByName, DateTimeOffset? ResolvedAt, DateTimeOffset? ClosedAt, string? RejectionReason, bool IsOverdue, IReadOnlyCollection<BankComplaintNoteDto> Notes, IReadOnlyCollection<BankComplaintAttachmentDto> Attachments, IReadOnlyCollection<BankComplaintHistoryDto> History, BankComplaintAccessDto Access);
public sealed record CreateBankComplaintRequest(Guid CaseId, [Required, MaxLength(100)] string Type, [Required, MaxLength(30)] string Priority, [Required, MaxLength(4000)] string Description, DateTimeOffset? DueAt, Guid? AssignedToId, [MaxLength(3000)] string? Note);
public sealed record AssignBankComplaintRequest(Guid AssignedToId, [MaxLength(500)] string? Reason, DateTimeOffset ExpectedUpdatedAt);
public sealed record ChangeBankComplaintPriorityRequest([Required, MaxLength(30)] string Priority, DateTimeOffset ExpectedUpdatedAt);
public sealed record AddBankComplaintNoteRequest([Required, MaxLength(3000)] string Text);
public sealed record ResolveBankComplaintRequest([Required, MaxLength(4000)] string Resolution, [MaxLength(3000)] string? Notes, DateTimeOffset ExpectedUpdatedAt);
public sealed record ComplaintReasonRequest([Required, MaxLength(2000)] string Reason, DateTimeOffset ExpectedUpdatedAt);
public sealed record ComplaintTransitionRequest(DateTimeOffset ExpectedUpdatedAt);
public sealed record BankComplaintAttachmentDownloadDto(Stream Content, string ContentType, string FileName);
