namespace MIS.Application.DTOs.Collections;

public sealed record BankDcrQuery(string? Search = null, DateOnly? Date = null, string? ActionCover = null, string? Action = null,
    Guid? CollectorId = null, string? SortBy = null, string? SortDirection = null, int Page = 1, int PageSize = 20);
public sealed record CreateBankDcrRequest(Guid CaseId, string ActionCover, string Action, string Feedback, string? Comment = null,
    DateOnly? PtpDate = null, decimal? PtpAmount = null, DateOnly? PaidDate = null, decimal? PaidAmount = null,
    DateTimeOffset? FollowUpAt = null, DateOnly? VisitDate = null, Guid? LinkedPtpId = null, Guid? LinkedVisitId = null);
public sealed record BankDcrAccessDto(bool IsManager, bool CanCreate, string TimeZoneId, DateOnly BusinessToday);
public sealed record BankDcrItemDto(Guid Id, Guid BankId, Guid CaseId, string CaseNumber, string CustomerName, string? Mobile,
    DateOnly DcrDate, string ActionCover, string Action, string Feedback, string? Comment, Guid CreatedByUserId, string CreatedBy,
    DateOnly? PtpDate, decimal? PtpAmount, DateOnly? PaidDate, decimal? PaidAmount, DateTimeOffset? FollowUpAt,
    DateOnly? VisitDate, Guid? LinkedPtpId, string? LinkedPtpStatus, Guid? LinkedVisitId, DateTimeOffset CreatedAt);
public sealed record BankDcrPageDto(IReadOnlyCollection<BankDcrItemDto> Items, int TotalCount, int Page, int PageSize,
    int TotalPages, BankDcrAccessDto Access);
public sealed record BankDcrCollectorDto(Guid Id, string Name);
