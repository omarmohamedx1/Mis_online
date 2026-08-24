using System.ComponentModel.DataAnnotations;

namespace MIS.Application.DTOs.Collections;

public sealed record BankVisitQuery(int Page = 1, int PageSize = 20, string? Search = null, string? View = null,
    string? Status = null, DateOnly? VisitDate = null, string? Result = null, Guid? CollectorId = null,
    string? SortBy = null, string? SortDirection = null);
public sealed record BankVisitAccessDto(bool IsManager, bool CanCreate, bool CanManageAssignment);
public sealed record BankVisitSummaryDto(int VisitsToday, int UpcomingVisits, int OverdueVisits, int CompletedToday);
public sealed record BankVisitItemDto(Guid Id, Guid CaseId, string CaseNumber, string CustomerName,
    DateTimeOffset ScheduledAt, Guid CollectorId, string CollectorName, string Status, string? Result,
    DateTimeOffset UpdatedAt);
public sealed record BankVisitPageDto(IReadOnlyCollection<BankVisitItemDto> Items, int TotalCount, int Page,
    int PageSize, int TotalPages, BankVisitAccessDto Access);
public sealed record BankVisitDetailsDto(Guid Id, Guid CaseId, string CaseNumber, string CustomerName, string? Mobile,
    string BankName, string Address, DateTimeOffset ScheduledAt, Guid CollectorId, string CollectorName, string Status,
    string? Result, string? Purpose, string? Notes, string CreatedBy, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
    DateTimeOffset? CompletedAt, BankVisitAccessDto Access);
public sealed record BankVisitCaseLookupDto(Guid Id, string CaseNumber, string CustomerName, string? Mobile,
    string? Address, string? Governorate, string? Area, Guid? AssignedCollectorId, string? AssignedCollectorName);
public sealed record CreateBankVisitRequest(Guid CaseId, Guid? AssignedCollectorId, DateTimeOffset ScheduledAt,
    [MaxLength(600)] string? Address, [MaxLength(500)] string? Purpose, [MaxLength(3000)] string? Notes);
public sealed record CompleteBankVisitRequest([Required, MaxLength(100)] string Result,
    [MaxLength(3000)] string? Notes, DateTimeOffset? NextFollowUpAt);
public sealed record RescheduleBankVisitRequest(DateTimeOffset ScheduledAt);
public sealed record ReassignBankVisitRequest(Guid CollectorId);
public sealed record CancelBankVisitRequest([MaxLength(3000)] string? Notes);
public sealed record ChangeBankVisitStatusRequest([Required, MaxLength(32)] string Status, [MaxLength(3000)] string? Notes);
