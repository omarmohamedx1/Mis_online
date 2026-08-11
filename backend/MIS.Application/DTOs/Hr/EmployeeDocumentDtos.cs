using System.ComponentModel.DataAnnotations;

namespace MIS.Application.DTOs.Hr;

public static class EmployeeDocumentExpiryFilters
{
    public const string All = "All";
    public const string Expired = "Expired";
    public const string ExpiringSoon = "ExpiringSoon";
    public const string Valid = "Valid";
    public const string NoExpiry = "NoExpiry";
}

public sealed class EmployeeDocumentFilterDto
{
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 20;
    [StringLength(160)] public string? Search { get; init; }
    public Guid? EmployeeId { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? DocumentTypeId { get; init; }
    [StringLength(24)] public string? ExpiryStatus { get; init; }
    [Range(1, 365)] public int ExpiringWithinDays { get; init; } = 30;
    [StringLength(32)] public string SortBy { get; init; } = "uploadedAt";
    [StringLength(4)] public string SortDirection { get; init; } = "desc";
}

public sealed record EmployeeDocumentListItemDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeNumber,
    string EmployeeName,
    string DepartmentName,
    Guid? DocumentTypeId,
    string DocumentType,
    string FileName,
    string MimeType,
    long FileSize,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    string ExpiryStatus,
    int? DaysUntilExpiry,
    string UploadedBy,
    DateTimeOffset UploadedAt,
    DateTimeOffset? UpdatedAt);

public sealed record EmployeeDocumentDetailsDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeNumber,
    string EmployeeName,
    Guid? DocumentTypeId,
    string DocumentType,
    string FileName,
    string MimeType,
    long FileSize,
    string? Sha256Hash,
    DateOnly? IssueDate,
    DateOnly? ExpiryDate,
    string ExpiryStatus,
    int? DaysUntilExpiry,
    string? Notes,
    Guid UploadedByUserId,
    string UploadedBy,
    DateTimeOffset UploadedAt,
    DateTimeOffset? UpdatedAt);

public sealed record PagedEmployeeDocumentsDto(
    IReadOnlyCollection<EmployeeDocumentListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed class CreateEmployeeDocumentRequest
{
    public Guid EmployeeId { get; init; }
    public Guid DocumentTypeId { get; init; }
    public DateOnly? IssueDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
    [StringLength(1000)] public string? Notes { get; init; }
}

public sealed class UpdateEmployeeDocumentRequest
{
    public Guid DocumentTypeId { get; init; }
    public DateOnly? IssueDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
    [StringLength(1000)] public string? Notes { get; init; }
}

public sealed class DeleteEmployeeDocumentRequest
{
    [StringLength(500)] public string? Reason { get; init; }
}

public sealed record HrUploadFile(
    string FileName,
    string ContentType,
    long Length,
    Stream Content);

public sealed record EmployeeDocumentFile(
    Stream Content,
    string FileName,
    string ContentType);

public sealed record DocumentExpirySummaryDto(
    int Expired,
    int ExpiringWithin7Days,
    int ExpiringWithin15Days,
    int ExpiringWithin30Days);
