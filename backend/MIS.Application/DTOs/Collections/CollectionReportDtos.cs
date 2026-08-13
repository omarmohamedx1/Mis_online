namespace MIS.Application.DTOs.Collections;

public sealed record CollectionReportFilters(Guid? OrganizationId, Guid? PortfolioId, Guid? CollectorId, DateOnly From, DateOnly To);
public sealed record CollectionReportSummaryDto(int TotalCases, decimal Outstanding, decimal Overdue, decimal ApprovedCollection, decimal PromiseAmount, decimal FulfilledPromiseAmount, decimal RecoveryRate, decimal PromiseFulfillmentRate);
public sealed record CollectionReportBreakdownDto(string Code, string Name, int Cases, decimal Outstanding, decimal Overdue, decimal Collected, decimal PromiseAmount, decimal FulfilledPromiseAmount);
public sealed record CollectionReportDto(CollectionReportFilters Filters, CollectionReportSummaryDto Summary, IReadOnlyCollection<CollectionReportBreakdownDto> ByClient, IReadOnlyCollection<CollectionReportBreakdownDto> ByBucket, IReadOnlyCollection<CollectionReportBreakdownDto> ByCollector);
