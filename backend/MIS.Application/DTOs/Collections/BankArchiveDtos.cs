namespace MIS.Application.DTOs.Collections;

public sealed record ArchiveSummaryDto(int ArchivedCases, int ArchivedPortfolios, int ArchivedThisMonth, int RestoredThisMonth, bool CanManage);
public sealed record ArchiveCaseQuery(int Page = 1, int PageSize = 20, string? Search = null, DateOnly? ArchivedFrom = null, DateOnly? ArchivedTo = null, Guid? ArchivedById = null, Guid? PreviousCollectorId = null, Guid? ImportId = null, string? Reason = null, string? SortBy = null, string? SortDirection = null);
public sealed record ArchiveCaseItemDto(Guid Id, string CaseNumber, string CustomerName, string? Mobile, decimal OutstandingAmount, Guid? PreviousCollectorId, string? PreviousCollector, string Reason, string ArchivedBy, DateTimeOffset ArchivedAt, DateTimeOffset UpdatedAt);
public sealed record ArchiveCasePageDto(IReadOnlyCollection<ArchiveCaseItemDto> Items, int TotalCount, int Page, int PageSize, int TotalPages, bool CanManage);
public sealed record ArchiveHistoryItemDto(string Action, string? Reason, string? Notes, string? PerformedBy, DateTimeOffset OccurredAt);
public sealed record ArchiveRelatedItemDto(Guid Id, string Type, string Status, string? Result, string? Notes, DateTimeOffset OccurredAt);
public sealed record ArchiveCaseDetailsDto(Guid Id, string CaseNumber, string CustomerName, string CustomerCode, string? Mobile, string? NationalId, string? Address, string BankName, string PortfolioName, decimal OriginalAmount, decimal OutstandingAmount, string Status, string Reason, string? Notes, string ArchivedBy, DateTimeOffset ArchivedAt, string? PreviousCollector, IReadOnlyCollection<ArchiveHistoryItemDto> Lifecycle, IReadOnlyCollection<ArchiveRelatedItemDto> Activities, IReadOnlyCollection<ArchiveRelatedItemDto> Ptps, IReadOnlyCollection<ArchiveRelatedItemDto> Visits, IReadOnlyCollection<ArchiveRelatedItemDto> Complaints, IReadOnlyCollection<ArchiveRelatedItemDto> Attachments, bool CanRestore, DateTimeOffset UpdatedAt);
public sealed record ArchivePortfolioQuery(int Page = 1, int PageSize = 20, string? Search = null, DateOnly? ArchivedFrom = null, DateOnly? ArchivedTo = null, Guid? ArchivedById = null, string? SortBy = null, string? SortDirection = null);
public sealed record ArchivePortfolioItemDto(Guid Id, string PortfolioName, string OriginalFileName, int Records, DateTimeOffset ImportDate, string Reason, string ArchivedBy, DateTimeOffset ArchivedAt);
public sealed record ArchivePortfolioPageDto(IReadOnlyCollection<ArchivePortfolioItemDto> Items, int TotalCount, int Page, int PageSize, int TotalPages, bool CanManage);
public sealed record ArchiveMutationRequest(string Reason, string? Notes, DateTimeOffset? ExpectedUpdatedAt = null);
public sealed record RestoreMutationRequest(string Reason, DateTimeOffset? ExpectedUpdatedAt = null);
public sealed record ArchiveFilterOptionDto(Guid Id, string Name);
