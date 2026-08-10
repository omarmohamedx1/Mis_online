namespace MIS.Application.DTOs.Hr;

public sealed record DepartmentEmployeeCountDto(
    Guid DepartmentId,
    string DepartmentName,
    string DepartmentCode,
    int EmployeeCount);

public sealed record HrDashboardDto(
    int TotalEmployees,
    int ActiveEmployees,
    int? AbsentToday,
    bool AttendanceAvailable,
    int? DocumentsRequiringAttention,
    bool DocumentAttentionAvailable,
    int TotalDocuments,
    IReadOnlyCollection<DepartmentEmployeeCountDto> EmployeesByDepartment);
