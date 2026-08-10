using System.ComponentModel.DataAnnotations;

namespace MIS.Application.DTOs.Hr;

public sealed record AbsenceListItemDto(Guid Id, Guid EmployeeId, string EmployeeNumber, string EmployeeName, Guid DepartmentId, string DepartmentName, DateOnly AbsenceDate, string Type, string Status);
public sealed record AbsenceDetailsDto(Guid Id, Guid EmployeeId, string EmployeeNumber, string EmployeeName, Guid DepartmentId, string DepartmentName, DateOnly AbsenceDate, string Type, string? Reason, string Status, string? Notes, string AttendanceSource, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
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
