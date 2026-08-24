using System.ComponentModel.DataAnnotations;

namespace MIS.Application.DTOs.Collections;

public sealed record BankCaseActivityQuery(int Page = 1, int PageSize = 20, string? Search = null,
    string? ActivityType = null, string? Outcome = null, DateOnly? Date = null, string? FollowUpState = null,
    Guid? CollectorId = null, Guid? CaseId = null);
public sealed record BankCaseActivityAccessDto(bool IsManager, bool CanCreate, IReadOnlyCollection<string> ActivityTypes,
    IReadOnlyCollection<string> CallOutcomes, IReadOnlyCollection<string> FilterActivityTypes);
public sealed record BankCaseActivitySummaryDto(int ActivitiesToday, int FollowUpsToday, int OverdueFollowUps, int CasesContactedToday);
public sealed record BankCaseActivityItemDto(Guid Id, Guid CaseId, string CaseNumber, string CustomerName,
    string ActivityType, string? Outcome, string? Notes, DateTimeOffset ActivityAt, DateTimeOffset? NextFollowUpAt,
    Guid PerformedById, string PerformedBy, BankCaseActivityAccessDto Access);
public sealed record BankCaseActivityPageDto(IReadOnlyCollection<BankCaseActivityItemDto> Items, int TotalCount,
    int Page, int PageSize, int TotalPages, BankCaseActivityAccessDto Access);
public sealed record BankCaseActivityDetailsDto(Guid Id, Guid CaseId, string CaseNumber, string CustomerName,
    string? Mobile, decimal OutstandingAmount, string CaseStatus, string BankName, Guid? AssignedCollectorId,
    string? AssignedCollectorName, string ActivityType, string? Outcome, string? Notes, DateTimeOffset ActivityAt,
    DateTimeOffset? NextFollowUpAt, Guid PerformedById, string PerformedBy, DateTimeOffset CreatedAt,
    IReadOnlyCollection<BankCaseActivityItemDto> Timeline, BankCaseActivityAccessDto Access);
public sealed record BankActivityCaseLookupDto(Guid Id, string CaseNumber, string CustomerName, string? Mobile,
    decimal OutstandingAmount, string Status, Guid? AssignedCollectorId, string? AssignedCollectorName);
public sealed record CreateBankCaseActivityRequest(Guid CaseId, [Required, MaxLength(40)] string ActivityType,
    [MaxLength(100)] string? Outcome, [MaxLength(4000)] string? Notes, DateTimeOffset? NextFollowUpAt);
