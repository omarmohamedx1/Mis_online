using System.ComponentModel.DataAnnotations;

namespace MIS.Application.DTOs.Hr;

public static class HrDelegationStatuses
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Cancelled = "Cancelled";
}

public sealed class DelegationFilterDto
{
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 20;
    [StringLength(160)] public string? Search { get; init; }
    public Guid? EmployeeId { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? DelegationTypeId { get; init; }
    public Guid? DelegatingEntityId { get; init; }
    [StringLength(24)] public string? Status { get; init; }
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTo { get; init; }
    [StringLength(32)] public string SortBy { get; init; } = "createdAt";
    [StringLength(4)] public string SortDirection { get; init; } = "desc";
}

public sealed record DelegationListItemDto(
    Guid Id,
    string DelegationNumber,
    Guid EmployeeId,
    string EmployeeNumber,
    string EmployeeName,
    string DepartmentName,
    Guid DelegationTypeId,
    string DelegationType,
    string Subject,
    Guid? DelegatingEntityId,
    string? AuthorizedEntity,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    string CreatedBy,
    DateTimeOffset CreatedAt);

public sealed record DelegationDetailsDto(
    Guid Id,
    string DelegationNumber,
    Guid EmployeeId,
    string EmployeeNumber,
    string EmployeeName,
    string? EmployeeNationalId,
    string DepartmentName,
    Guid DelegationTypeId,
    string DelegationType,
    string Subject,
    Guid? DelegatingEntityId,
    string? AuthorizedEntity,
    string? CompanyRepresentative,
    string? PowerOfAttorneyNumber,
    int? PowerOfAttorneyYear,
    DateOnly StartDate,
    DateOnly EndDate,
    string Purpose,
    string? Notes,
    string Status,
    Guid CreatedByUserId,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? CancellationReason,
    DateTimeOffset? CancelledAt);

public sealed record PagedDelegationsDto(
    IReadOnlyCollection<DelegationListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed class CreateDelegationRequest
{
    public Guid EmployeeId { get; init; }
    public Guid? DelegationTypeId { get; init; }
    [StringLength(250)] public string? Subject { get; init; }
    public Guid? DelegatingEntityId { get; init; }
    [Required, StringLength(250, MinimumLength = 2)] public string AuthorizedEntity { get; init; } = string.Empty;
    [StringLength(250)] public string? CompanyRepresentative { get; init; }
    [StringLength(100)] public string? PowerOfAttorneyNumber { get; init; }
    [Range(1900, 9999)] public int? PowerOfAttorneyYear { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    [Required, StringLength(4000, MinimumLength = 2)] public string Purpose { get; init; } = string.Empty;
    [StringLength(2000)] public string? Notes { get; init; }
    [Required, StringLength(24)] public string Status { get; init; } = HrDelegationStatuses.Draft;
}

public sealed class UpdateDelegationRequest
{
    public Guid? EmployeeId { get; init; }
    public Guid? DelegationTypeId { get; init; }
    [StringLength(250)] public string? Subject { get; init; }
    public Guid? DelegatingEntityId { get; init; }
    [Required, StringLength(250, MinimumLength = 2)] public string AuthorizedEntity { get; init; } = string.Empty;
    [StringLength(250)] public string? CompanyRepresentative { get; init; }
    [StringLength(100)] public string? PowerOfAttorneyNumber { get; init; }
    [Range(1900, 9999)] public int? PowerOfAttorneyYear { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    [Required, StringLength(4000, MinimumLength = 2)] public string Purpose { get; init; } = string.Empty;
    [StringLength(2000)] public string? Notes { get; init; }
    [Required, StringLength(24)] public string Status { get; init; } = HrDelegationStatuses.Draft;
}

public sealed class CancelDelegationRequest
{
    [Required, StringLength(500, MinimumLength = 2)] public string Reason { get; init; } = string.Empty;
}

public sealed record DelegationPrintDto(
    string DelegationNumber,
    string EmployeeName,
    string EmployeeNumber,
    string? NationalId,
    string? CompanyRepresentative,
    string? PowerOfAttorneyNumber,
    int? PowerOfAttorneyYear,
    string? AuthorizedEntity,
    string Purpose,
    DateOnly StartDate,
    DateOnly EndDate,
    DateTimeOffset CreatedAt);

public sealed record DelegationEntityOptionDto(Guid Id, string NameArabic, string NameEnglish);
