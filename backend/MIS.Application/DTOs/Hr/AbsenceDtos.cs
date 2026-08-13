using System.ComponentModel.DataAnnotations;

namespace MIS.Application.DTOs.Hr;

public sealed record AbsenceListItemDto(Guid Id, Guid EmployeeId, string EmployeeNumber, string EmployeeName, Guid DepartmentId, string DepartmentName, DateOnly AbsenceDate, string Type, string Status, decimal SuggestedDeductionAmount, decimal? ApprovedDeductionAmount, string PayrollImpactStatus);
public sealed record AbsenceDetailsDto(Guid Id, Guid EmployeeId, string EmployeeNumber, string EmployeeName, Guid DepartmentId, string DepartmentName, DateOnly AbsenceDate, string Type, string? Reason, string Status, string? Notes, string AttendanceSource, decimal SuggestedDeductionAmount, decimal? ApprovedDeductionAmount, string PayrollImpactStatus, string? PayrollNotes, string? PayrollReviewedByUsername, DateTimeOffset? PayrollReviewedAt, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record PagedAbsencesDto(IReadOnlyCollection<AbsenceListItemDto> Items, int TotalCount, int Page, int PageSize, int TotalPages);

public sealed class SaveAbsenceRequest
{
    [Required] public Guid EmployeeId { get; init; }
    [Required] public DateOnly AbsenceDate { get; init; }
    [Required, StringLength(24)] public string Type { get; init; } = "Absent";
    [StringLength(500)] public string? Reason { get; init; }
    [Required, StringLength(24)] public string Status { get; init; } = "Pending";
    [StringLength(2000)] public string? Notes { get; init; }
    [Required, StringLength(24)] public string AttendanceSource { get; init; } = "Manual";
}

public sealed class ReviewAbsencePayrollImpactRequest
{
    [Required, RegularExpression("^(Approve|Exclude)$")] public string Decision { get; init; } = string.Empty;
    [Range(typeof(decimal), "0", "9999999999999999")] public decimal? ApprovedDeductionAmount { get; init; }
    [StringLength(1000)] public string? Notes { get; init; }
}
