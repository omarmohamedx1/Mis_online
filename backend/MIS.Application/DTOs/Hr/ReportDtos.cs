using System.ComponentModel.DataAnnotations;

namespace MIS.Application.DTOs.Hr;

public static class HrReportCodes
{
    public const string EmployeeList = "employee-list";
    public const string EmployeeDetails = "employee-details";
    public const string Attendance = "attendance";
    public const string Absence = "absence";
    public const string Leave = "leave";
    public const string LateEmployees = "late-employees";
    public const string Overtime = "overtime";
    public const string ExpiringContracts = "expiring-contracts";
    public const string ExpiringDocuments = "expiring-documents";
    public const string EmployeesByDepartment = "employees-by-department";
    public const string Delegations = "delegations";

    public static readonly IReadOnlyCollection<string> All =
    [
        EmployeeList,
        EmployeeDetails,
        Attendance,
        Absence,
        Leave,
        LateEmployees,
        Overtime,
        ExpiringContracts,
        ExpiringDocuments,
        EmployeesByDepartment,
        Delegations
    ];
}

public static class HrReportExportFormats
{
    public const string Excel = "excel";
    public const string Pdf = "pdf";
}

public sealed record HrReportCatalogItemDto(
    string Code,
    string Name,
    string Description,
    IReadOnlyCollection<string> SupportedFilters);

public sealed record HrReportColumnDto(string Key, string Header);

public sealed record HrReportRowDto(IReadOnlyDictionary<string, string?> Values);

public sealed record HrReportPreviewDto(
    string ReportCode,
    string ReportName,
    IReadOnlyCollection<HrReportColumnDto> Columns,
    IReadOnlyCollection<HrReportRowDto> Rows,
    IReadOnlyDictionary<string, string> AppliedFilters,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    DateTimeOffset GeneratedAt);

public sealed record HrReportFileDto(
    string FileName,
    string ContentType,
    byte[] Content,
    int RowCount,
    DateTimeOffset GeneratedAt);

public sealed class HrReportFilterDto
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 200)]
    public int PageSize { get; init; } = 50;

    [StringLength(160)]
    public string? Search { get; init; }

    public DateOnly? DateFrom { get; init; }

    public DateOnly? DateTo { get; init; }

    public Guid? EmployeeId { get; init; }

    public Guid? DepartmentId { get; init; }

    public Guid? BranchId { get; init; }

    [StringLength(32)]
    public string? Status { get; init; }

    public Guid? TypeId { get; init; }

    [StringLength(100)]
    public string? Type { get; init; }
}
