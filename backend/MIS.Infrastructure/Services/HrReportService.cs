using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MIS.Application.Common;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;
using MIS.Domain.Constants;
using MIS.Domain.Entities;
using MIS.Infrastructure.Persistence;

namespace MIS.Infrastructure.Services;

public sealed class HrReportService : IHrReportService
{
    private const int MaximumPageSize = 200;
    private const int MaximumDateRangeDays = 366;
    private const int MaximumExportRows = 5000;
    private static readonly IReadOnlyCollection<HrReportCatalogItemDto> Catalog =
    [
        CatalogItem(HrReportCodes.EmployeeList, "Employee List", "Current employee directory and organization assignments.", "search", "employee", "department", "status"),
        CatalogItem(HrReportCodes.EmployeeDetails, "Employee Details", "Detailed employee profile without compensation or bank information.", "search", "employee", "department", "status"),
        CatalogItem(HrReportCodes.Attendance, "Attendance Report", "Attendance records and calculated working-time values.", "search", "dateFrom", "dateTo", "employee", "department", "status", "type"),
        CatalogItem(HrReportCodes.Absence, "Absence Report", "Registered company absences and their reviewed payroll impact.", "search", "dateFrom", "dateTo", "employee", "department", "status", "type"),
        CatalogItem(HrReportCodes.Leave, "Leave Report", "Leave requests and decisions.", "search", "dateFrom", "dateTo", "employee", "department", "status", "typeId", "type"),
        CatalogItem(HrReportCodes.LateEmployees, "Late Employees", "Attendance records with calculated late minutes.", "search", "dateFrom", "dateTo", "employee", "department"),
        CatalogItem(HrReportCodes.Overtime, "Overtime Report", "Attendance records with approved calculated overtime.", "search", "dateFrom", "dateTo", "employee", "department"),
        CatalogItem(HrReportCodes.ExpiringContracts, "Expiring Contracts", "Contracts that have expired recently or are approaching their end date.", "search", "dateFrom", "dateTo", "employee", "department", "status", "typeId", "type"),
        CatalogItem(HrReportCodes.ExpiringDocuments, "Expiring Documents", "Employee documents that have expired recently or will expire soon.", "search", "dateFrom", "dateTo", "employee", "department", "status", "typeId", "type"),
        CatalogItem(HrReportCodes.EmployeesByDepartment, "Employees by Department", "Employee totals grouped by department.", "search", "department", "status"),
        CatalogItem(HrReportCodes.Delegations, "Delegations Report", "Administrative delegations and their effective periods.", "search", "dateFrom", "dateTo", "employee", "department", "status", "typeId", "type")
    ];

    private readonly ApplicationDbContext _dbContext;

    public HrReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyCollection<HrReportCatalogItemDto> GetCatalog() => Catalog
        .Select(item => item with
        {
            Name = ApiTextLocalizer.Localize(item.Name),
            Description = ApiTextLocalizer.Localize(item.Description)
        })
        .ToArray();

    public async Task<HrReportPreviewDto> GetPreviewAsync(
        string reportCode,
        HrReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        var definition = GetDefinition(reportCode);
        ValidateFilter(filter);
        var generatedAt = DateTimeOffset.UtcNow;
        var data = await BuildReportAsync(
            definition.Code,
            filter,
            (filter.Page - 1) * filter.PageSize,
            filter.PageSize,
            null,
            cancellationToken);
        var appliedFilters = await BuildAppliedFiltersAsync(definition.Code, filter, cancellationToken);
        return new HrReportPreviewDto(
            definition.Code,
            ApiTextLocalizer.Localize(definition.Name),
            data.Columns,
            data.Rows,
            appliedFilters,
            data.TotalCount,
            filter.Page,
            filter.PageSize,
            Pages(data.TotalCount, filter.PageSize),
            generatedAt);
    }

    public async Task<HrReportFileDto> ExportAsync(
        string reportCode,
        string format,
        HrReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        var definition = GetDefinition(reportCode);
        ValidateFilter(filter);
        var normalizedFormat = format?.Trim().ToLowerInvariant();
        if (normalizedFormat is not HrReportExportFormats.Excel and not HrReportExportFormats.Pdf)
            throw new HrValidationException("Report export format must be excel or pdf.");

        var generatedAt = DateTimeOffset.UtcNow;
        var data = await BuildReportAsync(
            definition.Code,
            filter,
            0,
            MaximumExportRows,
            MaximumExportRows,
            cancellationToken);
        var appliedFilters = await BuildAppliedFiltersAsync(definition.Code, filter, cancellationToken);
        var localizedReportName = ApiTextLocalizer.Localize(definition.Name);
        var content = normalizedFormat == HrReportExportFormats.Excel
            ? HrReportExportWriter.WriteExcel(localizedReportName, data.Columns, data.Rows, appliedFilters, generatedAt)
            : HrReportExportWriter.WritePdf(localizedReportName, data.Columns, data.Rows, appliedFilters, generatedAt);
        var extension = normalizedFormat == HrReportExportFormats.Excel ? "xlsx" : "pdf";
        var contentType = normalizedFormat == HrReportExportFormats.Excel
            ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            : "application/pdf";
        return new HrReportFileDto(
            BuildExportFileName(localizedReportName, extension, generatedAt),
            contentType,
            content,
            data.Rows.Count,
            generatedAt);
    }

    private static string BuildExportFileName(string reportName, string extension, DateTimeOffset generatedAt)
    {
        var safeName = string.Concat(reportName.Trim().Select(character =>
            char.IsLetterOrDigit(character) ? character : '-'));
        while (safeName.Contains("--", StringComparison.Ordinal))
            safeName = safeName.Replace("--", "-", StringComparison.Ordinal);
        safeName = safeName.Trim('-');
        if (safeName.Length == 0) safeName = ApiTextLocalizer.IsArabic ? "تقرير" : "Report";
        return $"{safeName}-{generatedAt:yyyyMMdd-HHmmss}.{extension}";
    }

    private Task<ReportData> BuildReportAsync(
        string code,
        HrReportFilterDto filter,
        int skip,
        int take,
        int? maximumTotal,
        CancellationToken cancellationToken) => code switch
    {
        HrReportCodes.EmployeeList => BuildEmployeeListAsync(filter, skip, take, maximumTotal, cancellationToken),
        HrReportCodes.EmployeeDetails => BuildEmployeeDetailsAsync(filter, skip, take, maximumTotal, cancellationToken),
        HrReportCodes.Attendance => BuildAttendanceAsync(filter, skip, take, maximumTotal, AttendanceMode.All, cancellationToken),
        HrReportCodes.Absence => BuildAbsenceAsync(filter, skip, take, maximumTotal, cancellationToken),
        HrReportCodes.Leave => BuildLeaveAsync(filter, skip, take, maximumTotal, cancellationToken),
        HrReportCodes.LateEmployees => BuildAttendanceAsync(filter, skip, take, maximumTotal, AttendanceMode.Late, cancellationToken),
        HrReportCodes.Overtime => BuildAttendanceAsync(filter, skip, take, maximumTotal, AttendanceMode.Overtime, cancellationToken),
        HrReportCodes.ExpiringContracts => BuildContractsAsync(filter, skip, take, maximumTotal, cancellationToken),
        HrReportCodes.ExpiringDocuments => BuildDocumentsAsync(filter, skip, take, maximumTotal, cancellationToken),
        HrReportCodes.EmployeesByDepartment => BuildDepartmentSummaryAsync(filter, skip, take, maximumTotal, cancellationToken),
        HrReportCodes.Delegations => BuildDelegationsAsync(filter, skip, take, maximumTotal, cancellationToken),
        _ => throw new HrNotFoundException("Report was not found.")
    };

    private async Task<ReportData> BuildEmployeeListAsync(
        HrReportFilterDto filter,
        int skip,
        int take,
        int? maximumTotal,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var columns = Columns(
            ("employeeNumber", "Employee ID"), ("employeeName", "Employee Name"),
            ("department", "Department"), ("position", "Position"),
            ("hireDate", "Hire Date"), ("status", "Status"));
        var query = ApplyEmployeeFilters(_dbContext.Employees.AsNoTracking(), filter)
            .OrderBy(item => item.EmployeeNumber)
            .Select(item => new
            {
                item.EmployeeNumber,
                EmployeeName = isArabic ? item.FullNameArabic ?? item.FullName : item.FullNameEnglish ?? item.FullName,
                Department = isArabic ? item.Department.NameArabic ?? item.Department.Name : item.Department.Name,
                Position = item.Position == null ? null : isArabic ? item.Position.NameArabic ?? item.Position.Name : item.Position.Name,
                Branch = item.Branch == null ? null : isArabic ? item.Branch.NameArabic ?? item.Branch.Name : item.Branch.Name,
                item.HireDate,
                item.Status
            });
        return await PageAsync("Employee List", columns, query, skip, take, maximumTotal, item => Row(
            ("employeeNumber", item.EmployeeNumber), ("employeeName", item.EmployeeName),
            ("department", item.Department), ("position", item.Position),
            ("hireDate", item.HireDate), ("status", item.Status)), cancellationToken);
    }

    private async Task<ReportData> BuildEmployeeDetailsAsync(
        HrReportFilterDto filter,
        int skip,
        int take,
        int? maximumTotal,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var columns = Columns(
            ("employeeNumber", "Employee ID"), ("employeeName", "Employee Name"),
            ("nameArabic", "Arabic Name"), ("nameEnglish", "English Name"), ("nationalId", "National ID"),
            ("dateOfBirth", "Date of Birth"), ("gender", "Gender"), ("maritalStatus", "Marital Status"),
            ("mobile", "Mobile"), ("email", "Email"), ("city", "City"),
            ("department", "Department"), ("position", "Position"),
            ("manager", "Direct Manager"), ("employmentType", "Employment Type"),
            ("hireDate", "Hire Date"), ("status", "Status"), ("contractType", "Contract Type"),
            ("contractStart", "Contract Start"), ("contractEnd", "Contract End"), ("probationEnd", "Probation End"));
        var employees = ApplyEmployeeFilters(_dbContext.Employees.AsNoTracking(), filter);
        var query =
            from employee in employees
            from contract in _dbContext.EmployeeContracts.AsNoTracking()
                .Where(item => item.EmployeeId == employee.Id)
                .OrderByDescending(item => item.ContractStartDate)
                .Take(1)
                .DefaultIfEmpty()
            orderby employee.EmployeeNumber
            select new
            {
                employee.EmployeeNumber,
                EmployeeName = isArabic ? employee.FullNameArabic ?? employee.FullName : employee.FullNameEnglish ?? employee.FullName,
                employee.FullNameArabic,
                employee.FullNameEnglish,
                employee.NationalId,
                employee.DateOfBirth,
                employee.Gender,
                employee.MaritalStatus,
                employee.MobileNumber,
                employee.Email,
                employee.City,
                Department = isArabic ? employee.Department.NameArabic ?? employee.Department.Name : employee.Department.Name,
                Position = employee.Position == null ? null : isArabic ? employee.Position.NameArabic ?? employee.Position.Name : employee.Position.Name,
                Branch = employee.Branch == null ? null : isArabic ? employee.Branch.NameArabic ?? employee.Branch.Name : employee.Branch.Name,
                Manager = employee.DirectManager == null ? null : isArabic ? employee.DirectManager.FullNameArabic ?? employee.DirectManager.FullName : employee.DirectManager.FullNameEnglish ?? employee.DirectManager.FullName,
                EmploymentType = employee.EmploymentType == null ? null : isArabic ? employee.EmploymentType.NameArabic ?? employee.EmploymentType.Name : employee.EmploymentType.Name,
                employee.HireDate,
                employee.Status,
                ContractType = contract == null ? null : isArabic ? contract.ContractType.NameArabic ?? contract.ContractType.Name : contract.ContractType.Name,
                ContractStart = contract == null ? (DateOnly?)null : contract.ContractStartDate,
                ContractEnd = contract == null ? null : contract.ContractEndDate,
                ProbationEnd = contract == null ? null : contract.ProbationEndDate
            };
        return await PageAsync("Employee Details", columns, query, skip, take, maximumTotal, item => Row(
            ("employeeNumber", item.EmployeeNumber), ("employeeName", item.EmployeeName),
            ("nameArabic", item.FullNameArabic), ("nameEnglish", item.FullNameEnglish), ("nationalId", item.NationalId),
            ("dateOfBirth", item.DateOfBirth), ("gender", item.Gender), ("maritalStatus", item.MaritalStatus),
            ("mobile", item.MobileNumber), ("email", item.Email), ("city", item.City),
            ("department", item.Department), ("position", item.Position),
            ("manager", item.Manager), ("employmentType", item.EmploymentType), ("hireDate", item.HireDate),
            ("status", item.Status), ("contractType", item.ContractType), ("contractStart", item.ContractStart),
            ("contractEnd", item.ContractEnd), ("probationEnd", item.ProbationEnd)), cancellationToken);
    }

    private async Task<ReportData> BuildAttendanceAsync(
        HrReportFilterDto filter,
        int skip,
        int take,
        int? maximumTotal,
        AttendanceMode mode,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var reportName = mode switch
        {
            AttendanceMode.Late => "Late Employees",
            AttendanceMode.Overtime => "Overtime Report",
            _ => "Attendance Report"
        };
        var columns = Columns(
            ("date", "Date"), ("employeeNumber", "Employee ID"), ("employeeName", "Employee Name"),
            ("department", "Department"), ("checkIn", "Check In"),
            ("checkOut", "Check Out"), ("workingHours", "Working Hours"), ("lateMinutes", "Late Minutes"),
            ("earlyLeaveMinutes", "Early Leave Minutes"), ("overtimeMinutes", "Overtime Minutes"),
            ("status", "Status"), ("source", "Source"));
        var query = _dbContext.AttendanceRecords.AsNoTracking().Where(item => !item.IsDeleted);
        query = ApplyAttendanceFilters(query, filter);
        if (mode == AttendanceMode.Late) query = query.Where(item => item.LateMinutes > 0);
        if (mode == AttendanceMode.Overtime) query = query.Where(item => item.OvertimeMinutes > 0);
        var projected = query
            .OrderByDescending(item => item.AttendanceDate)
            .ThenBy(item => item.Employee.EmployeeNumber)
            .Select(item => new
            {
                Date = item.AttendanceDate,
                item.Employee.EmployeeNumber,
                EmployeeName = isArabic ? item.Employee.FullNameArabic ?? item.Employee.FullName : item.Employee.FullNameEnglish ?? item.Employee.FullName,
                Department = isArabic ? item.Employee.Department.NameArabic ?? item.Employee.Department.Name : item.Employee.Department.Name,
                Branch = item.Employee.Branch == null ? null : isArabic ? item.Employee.Branch.NameArabic ?? item.Employee.Branch.Name : item.Employee.Branch.Name,
                item.CheckIn,
                item.CheckOut,
                item.WorkingMinutes,
                item.LateMinutes,
                item.EarlyLeaveMinutes,
                item.OvertimeMinutes,
                item.Status,
                item.Source
            });
        return await PageAsync(reportName, columns, projected, skip, take, maximumTotal, item => Row(
            ("date", item.Date), ("employeeNumber", item.EmployeeNumber), ("employeeName", item.EmployeeName),
            ("department", item.Department), ("checkIn", item.CheckIn),
            ("checkOut", item.CheckOut), ("workingHours", decimal.Round(item.WorkingMinutes / 60m, 2)),
            ("lateMinutes", item.LateMinutes), ("earlyLeaveMinutes", item.EarlyLeaveMinutes),
            ("overtimeMinutes", item.OvertimeMinutes), ("status", item.Status), ("source", item.Source)), cancellationToken);
    }

    private async Task<ReportData> BuildAbsenceAsync(
        HrReportFilterDto filter,
        int skip,
        int take,
        int? maximumTotal,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var columns = Columns(
            ("date", "Date"), ("employeeNumber", "Employee ID"), ("employeeName", "Employee Name"),
            ("department", "Department"), ("type", "Type"),
            ("status", "Status"), ("payrollImpact", "Payroll Impact"),
            ("reason", "Reason"), ("source", "Source"));
        var query = _dbContext.EmployeeAbsences.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(item => EF.Functions.ILike(item.Employee.EmployeeNumber, pattern) ||
                                        EF.Functions.ILike(item.Employee.FullName, pattern) ||
                                        (item.Employee.FullNameArabic != null && EF.Functions.ILike(item.Employee.FullNameArabic, pattern)) ||
                                        (item.Employee.FullNameEnglish != null && EF.Functions.ILike(item.Employee.FullNameEnglish, pattern)));
        }
        query = ApplyEmployeeRelationFilters(query, filter, item => item.EmployeeId, item => item.Employee.DepartmentId, item => item.Employee.BranchId);
        if (filter.DateFrom.HasValue) query = query.Where(item => item.AbsenceDate >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue) query = query.Where(item => item.AbsenceDate <= filter.DateTo.Value);
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = NormalizeAbsenceStatus(filter.Status)
                ?? throw new HrValidationException("Absence status is invalid.");
            query = query.Where(item => item.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(filter.Type) && !filter.Type.Equals(AbsenceValues.AbsentType, StringComparison.OrdinalIgnoreCase))
            throw new HrValidationException("Absence type is invalid.");
        var projected = query.OrderByDescending(item => item.AbsenceDate).ThenBy(item => item.Employee.EmployeeNumber)
            .Select(item => new
            {
                Date = item.AbsenceDate,
                item.Employee.EmployeeNumber,
                EmployeeName = isArabic ? item.Employee.FullNameArabic ?? item.Employee.FullName : item.Employee.FullNameEnglish ?? item.Employee.FullName,
                Department = isArabic ? item.Employee.Department.NameArabic ?? item.Employee.Department.Name : item.Employee.Department.Name,
                Branch = item.Employee.Branch == null ? null : isArabic ? item.Employee.Branch.NameArabic ?? item.Employee.Branch.Name : item.Employee.Branch.Name,
                item.Type,
                item.Status,
                item.PayrollImpactStatus,
                item.Reason,
                Source = item.AttendanceSource
            });
        return await PageAsync("Absence Report", columns, projected, skip, take, maximumTotal, item => Row(
            ("date", item.Date), ("employeeNumber", item.EmployeeNumber), ("employeeName", item.EmployeeName),
            ("department", item.Department), ("type", item.Type),
            ("status", item.Status), ("payrollImpact", item.PayrollImpactStatus),
            ("reason", item.Reason), ("source", item.Source)), cancellationToken);
    }

    private async Task<ReportData> BuildLeaveAsync(
        HrReportFilterDto filter,
        int skip,
        int take,
        int? maximumTotal,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var columns = Columns(
            ("employeeNumber", "Employee ID"), ("employeeName", "Employee Name"),
            ("department", "Department"), ("leaveType", "Leave Type"),
            ("startDate", "Start Date"), ("endDate", "End Date"), ("days", "Days"),
            ("status", "Status"), ("requestDate", "Request Date"), ("reason", "Reason"),
            ("decision", "Decision Notes"));
        var query = _dbContext.LeaveRequests.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(item => EF.Functions.ILike(item.Employee.EmployeeNumber, pattern) ||
                                        EF.Functions.ILike(item.Employee.FullName, pattern) ||
                                        (item.Employee.FullNameArabic != null && EF.Functions.ILike(item.Employee.FullNameArabic, pattern)) ||
                                        (item.Employee.FullNameEnglish != null && EF.Functions.ILike(item.Employee.FullNameEnglish, pattern)) ||
                                        EF.Functions.ILike(item.LeaveType.Name, pattern) ||
                                        (item.LeaveType.NameArabic != null && EF.Functions.ILike(item.LeaveType.NameArabic, pattern)));
        }
        query = ApplyEmployeeRelationFilters(query, filter, item => item.EmployeeId, item => item.Employee.DepartmentId, item => item.Employee.BranchId);
        if (filter.DateFrom.HasValue) query = query.Where(item => item.EndDate >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue) query = query.Where(item => item.StartDate <= filter.DateTo.Value);
        if (filter.TypeId.HasValue) query = query.Where(item => item.LeaveTypeId == filter.TypeId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            var pattern = $"%{filter.Type.Trim()}%";
            query = query.Where(item => EF.Functions.ILike(item.LeaveType.Name, pattern) ||
                                        (item.LeaveType.NameArabic != null && EF.Functions.ILike(item.LeaveType.NameArabic, pattern)) ||
                                        EF.Functions.ILike(item.LeaveType.Code, pattern));
        }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = NormalizeLeaveStatus(filter.Status)
                ?? throw new HrValidationException("Leave status is invalid.");
            query = query.Where(item => item.Status == status);
        }
        var projected = query.OrderByDescending(item => item.RequestDate)
            .Select(item => new
            {
                item.Employee.EmployeeNumber,
                EmployeeName = isArabic ? item.Employee.FullNameArabic ?? item.Employee.FullName : item.Employee.FullNameEnglish ?? item.Employee.FullName,
                Department = isArabic ? item.Employee.Department.NameArabic ?? item.Employee.Department.Name : item.Employee.Department.Name,
                Branch = item.Employee.Branch == null ? null : isArabic ? item.Employee.Branch.NameArabic ?? item.Employee.Branch.Name : item.Employee.Branch.Name,
                LeaveType = isArabic ? item.LeaveType.NameArabic ?? item.LeaveType.Name : item.LeaveType.Name,
                item.StartDate,
                item.EndDate,
                Days = item.NumberOfDays,
                item.Status,
                item.RequestDate,
                item.Reason,
                Decision = item.DecisionNotes
            });
        return await PageAsync("Leave Report", columns, projected, skip, take, maximumTotal, item => Row(
            ("employeeNumber", item.EmployeeNumber), ("employeeName", item.EmployeeName),
            ("department", item.Department), ("leaveType", item.LeaveType),
            ("startDate", item.StartDate), ("endDate", item.EndDate), ("days", item.Days),
            ("status", item.Status), ("requestDate", item.RequestDate), ("reason", item.Reason),
            ("decision", item.Decision)), cancellationToken);
    }

    private async Task<ReportData> BuildContractsAsync(
        HrReportFilterDto filter,
        int skip,
        int take,
        int? maximumTotal,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = filter.DateFrom ?? today.AddDays(-30);
        var to = filter.DateTo ?? today.AddDays(90);
        var columns = Columns(
            ("employeeNumber", "Employee ID"), ("employeeName", "Employee Name"),
            ("department", "Department"), ("contractType", "Contract Type"),
            ("startDate", "Start Date"), ("endDate", "End Date"), ("daysRemaining", "Days Remaining"),
            ("status", "Status"));
        var query = _dbContext.EmployeeContracts.AsNoTracking().Where(item => item.ContractEndDate.HasValue &&
            item.ContractEndDate.Value >= from && item.ContractEndDate.Value <= to);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(item => EF.Functions.ILike(item.Employee.EmployeeNumber, pattern) ||
                                        EF.Functions.ILike(item.Employee.FullName, pattern) ||
                                        (item.Employee.FullNameArabic != null && EF.Functions.ILike(item.Employee.FullNameArabic, pattern)) ||
                                        (item.Employee.FullNameEnglish != null && EF.Functions.ILike(item.Employee.FullNameEnglish, pattern)) ||
                                        EF.Functions.ILike(item.ContractType.Name, pattern) ||
                                        (item.ContractType.NameArabic != null && EF.Functions.ILike(item.ContractType.NameArabic, pattern)));
        }
        query = ApplyEmployeeRelationFilters(query, filter, item => item.EmployeeId, item => item.Employee.DepartmentId, item => item.Employee.BranchId);
        if (filter.TypeId.HasValue) query = query.Where(item => item.ContractTypeId == filter.TypeId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            var pattern = $"%{filter.Type.Trim()}%";
            query = query.Where(item => EF.Functions.ILike(item.ContractType.Name, pattern) ||
                                        (item.ContractType.NameArabic != null && EF.Functions.ILike(item.ContractType.NameArabic, pattern)) ||
                                        EF.Functions.ILike(item.ContractType.Code, pattern));
        }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = NormalizeContractStatus(filter.Status)
                ?? throw new HrValidationException("Contract status is invalid.");
            query = query.Where(item => item.Status == status);
        }
        var projected = query.OrderBy(item => item.ContractEndDate).Select(item => new
        {
            item.Employee.EmployeeNumber,
            EmployeeName = isArabic ? item.Employee.FullNameArabic ?? item.Employee.FullName : item.Employee.FullNameEnglish ?? item.Employee.FullName,
            Department = isArabic ? item.Employee.Department.NameArabic ?? item.Employee.Department.Name : item.Employee.Department.Name,
            Branch = item.Employee.Branch == null ? null : isArabic ? item.Employee.Branch.NameArabic ?? item.Employee.Branch.Name : item.Employee.Branch.Name,
            ContractType = isArabic ? item.ContractType.NameArabic ?? item.ContractType.Name : item.ContractType.Name,
            StartDate = item.ContractStartDate,
            EndDate = item.ContractEndDate!.Value,
            item.Status
        });
        return await PageAsync("Expiring Contracts", columns, projected, skip, take, maximumTotal, item => Row(
            ("employeeNumber", item.EmployeeNumber), ("employeeName", item.EmployeeName),
            ("department", item.Department), ("contractType", item.ContractType),
            ("startDate", item.StartDate), ("endDate", item.EndDate),
            ("daysRemaining", item.EndDate.DayNumber - today.DayNumber), ("status", item.Status)), cancellationToken);
    }

    private async Task<ReportData> BuildDocumentsAsync(
        HrReportFilterDto filter,
        int skip,
        int take,
        int? maximumTotal,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = filter.DateFrom ?? today.AddDays(-30);
        var to = filter.DateTo ?? today.AddDays(90);
        var columns = Columns(
            ("employeeNumber", "Employee ID"), ("employeeName", "Employee Name"),
            ("department", "Department"), ("documentType", "Document Type"),
            ("fileName", "File Name"), ("issueDate", "Issue Date"), ("expiryDate", "Expiry Date"),
            ("daysRemaining", "Days Remaining"), ("status", "Status"));
        var query = _dbContext.EmployeeDocuments.AsNoTracking().Where(item => !item.IsDeleted && item.ExpiryDate.HasValue &&
            item.ExpiryDate.Value >= from && item.ExpiryDate.Value <= to);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(item => EF.Functions.ILike(item.Employee.EmployeeNumber, pattern) ||
                                        EF.Functions.ILike(item.Employee.FullName, pattern) ||
                                        (item.Employee.FullNameArabic != null && EF.Functions.ILike(item.Employee.FullNameArabic, pattern)) ||
                                        (item.Employee.FullNameEnglish != null && EF.Functions.ILike(item.Employee.FullNameEnglish, pattern)) ||
                                        EF.Functions.ILike(item.DocumentType, pattern) ||
                                        EF.Functions.ILike(item.FileName, pattern));
        }
        query = ApplyEmployeeRelationFilters(query, filter, item => item.EmployeeId, item => item.Employee.DepartmentId, item => item.Employee.BranchId);
        if (filter.TypeId.HasValue) query = query.Where(item => item.DocumentTypeId == filter.TypeId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            var pattern = $"%{filter.Type.Trim()}%";
            query = query.Where(item => EF.Functions.ILike(item.DocumentType, pattern));
        }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = filter.Status.Trim().ToLowerInvariant() switch
            {
                "expired" => query.Where(item => item.ExpiryDate < today),
                "expiringsoon" or "expiring_soon" or "expiring soon" => query.Where(item => item.ExpiryDate >= today && item.ExpiryDate <= today.AddDays(30)),
                "valid" => query.Where(item => item.ExpiryDate > today.AddDays(30)),
                _ => throw new HrValidationException("Document expiry status is invalid.")
            };
        }
        var projected = query.OrderBy(item => item.ExpiryDate).Select(item => new
        {
            item.Employee.EmployeeNumber,
            EmployeeName = isArabic ? item.Employee.FullNameArabic ?? item.Employee.FullName : item.Employee.FullNameEnglish ?? item.Employee.FullName,
            Department = isArabic ? item.Employee.Department.NameArabic ?? item.Employee.Department.Name : item.Employee.Department.Name,
            Branch = item.Employee.Branch == null ? null : isArabic ? item.Employee.Branch.NameArabic ?? item.Employee.Branch.Name : item.Employee.Branch.Name,
            DocumentType = isArabic && item.DocumentTypeDefinition != null
                ? item.DocumentTypeDefinition.NameArabic ?? item.DocumentTypeDefinition.Name
                : item.DocumentType,
            item.FileName,
            item.IssueDate,
            ExpiryDate = item.ExpiryDate!.Value
        });
        return await PageAsync("Expiring Documents", columns, projected, skip, take, maximumTotal, item =>
        {
            var days = item.ExpiryDate.DayNumber - today.DayNumber;
            var status = days < 0 ? "Expired" : days <= 30 ? "Expiring Soon" : "Valid";
            return Row(
                ("employeeNumber", item.EmployeeNumber), ("employeeName", item.EmployeeName),
                ("department", item.Department), ("documentType", item.DocumentType),
                ("fileName", item.FileName), ("issueDate", item.IssueDate), ("expiryDate", item.ExpiryDate),
                ("daysRemaining", days), ("status", status));
        }, cancellationToken);
    }

    private async Task<ReportData> BuildDepartmentSummaryAsync(
        HrReportFilterDto filter,
        int skip,
        int take,
        int? maximumTotal,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var columns = Columns(
            ("departmentCode", "Department Code"), ("department", "Department"),
            ("total", "Total Employees"), ("active", "Active"), ("inactive", "Inactive"));
        var employees = ApplyEmployeeFilters(_dbContext.Employees.AsNoTracking(), filter);
        var query = employees.GroupBy(item => new
            {
                item.DepartmentId,
                item.Department.Code,
                Name = isArabic ? item.Department.NameArabic ?? item.Department.Name : item.Department.Name
            })
            .Select(group => new
            {
                DepartmentCode = group.Key.Code,
                Department = group.Key.Name,
                Total = group.Count(),
                Active = group.Count(item => item.IsActive),
                Inactive = group.Count(item => !item.IsActive)
            })
            .OrderByDescending(item => item.Total)
            .ThenBy(item => item.Department);
        return await PageAsync("Employees by Department", columns, query, skip, take, maximumTotal, item => Row(
            ("departmentCode", item.DepartmentCode), ("department", item.Department),
            ("total", item.Total), ("active", item.Active), ("inactive", item.Inactive)), cancellationToken);
    }

    private async Task<ReportData> BuildBranchSummaryAsync(
        HrReportFilterDto filter,
        int skip,
        int take,
        int? maximumTotal,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var columns = Columns(
            ("branchCode", "Branch Code"), ("branch", "Branch"),
            ("total", "Total Employees"), ("active", "Active"), ("inactive", "Inactive"));
        var employees = ApplyEmployeeFilters(_dbContext.Employees.AsNoTracking(), filter);
        var query = employees.GroupBy(item => new
            {
                item.BranchId,
                Code = item.Branch == null ? null : item.Branch.Code,
                Name = item.Branch == null ? null : isArabic ? item.Branch.NameArabic ?? item.Branch.Name : item.Branch.Name
            })
            .Select(group => new
            {
                BranchCode = group.Key.Code,
                Branch = group.Key.Name ?? ApiTextLocalizer.Localize("Unassigned"),
                Total = group.Count(),
                Active = group.Count(item => item.IsActive),
                Inactive = group.Count(item => !item.IsActive)
            })
            .OrderByDescending(item => item.Total)
            .ThenBy(item => item.Branch);
        return await PageAsync("Employees by Branch", columns, query, skip, take, maximumTotal, item => Row(
            ("branchCode", item.BranchCode), ("branch", item.Branch),
            ("total", item.Total), ("active", item.Active), ("inactive", item.Inactive)), cancellationToken);
    }

    private async Task<ReportData> BuildDelegationsAsync(
        HrReportFilterDto filter,
        int skip,
        int take,
        int? maximumTotal,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var columns = Columns(
            ("delegationNumber", "Delegation Number"), ("employeeNumber", "Employee ID"),
            ("employeeName", "Employee Name"), ("department", "Department"),
            ("delegationType", "Delegation Type"), ("subject", "Subject"),
            ("authorizedEntity", "Authorized Entity"), ("startDate", "Start Date"),
            ("endDate", "End Date"), ("status", "Status"), ("createdAt", "Created At"));
        var query = _dbContext.EmployeeDelegations.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(item => EF.Functions.ILike(item.DelegationNumber, pattern) ||
                                        EF.Functions.ILike(item.Employee.EmployeeNumber, pattern) ||
                                        EF.Functions.ILike(item.Employee.FullName, pattern) ||
                                        (item.Employee.FullNameArabic != null && EF.Functions.ILike(item.Employee.FullNameArabic, pattern)) ||
                                        (item.Employee.FullNameEnglish != null && EF.Functions.ILike(item.Employee.FullNameEnglish, pattern)) ||
                                        EF.Functions.ILike(item.Subject, pattern));
        }
        query = ApplyEmployeeRelationFilters(query, filter, item => item.EmployeeId, item => item.Employee.DepartmentId, item => item.Employee.BranchId);
        if (filter.DateFrom.HasValue) query = query.Where(item => item.EndDate >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue) query = query.Where(item => item.StartDate <= filter.DateTo.Value);
        if (filter.TypeId.HasValue) query = query.Where(item => item.DelegationTypeId == filter.TypeId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            var pattern = $"%{filter.Type.Trim()}%";
            query = query.Where(item => EF.Functions.ILike(item.DelegationType.Name, pattern) ||
                                        (item.DelegationType.NameArabic != null && EF.Functions.ILike(item.DelegationType.NameArabic, pattern)) ||
                                        EF.Functions.ILike(item.DelegationType.Code, pattern));
        }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            string status;
            try { status = DelegationStatuses.Normalize(filter.Status); }
            catch (ArgumentException) { throw new HrValidationException("Delegation status is invalid."); }
            query = query.Where(item => item.Status == status);
        }
        var projected = query.OrderByDescending(item => item.CreatedAt).Select(item => new
        {
            item.DelegationNumber,
            item.Employee.EmployeeNumber,
            EmployeeName = isArabic ? item.Employee.FullNameArabic ?? item.Employee.FullName : item.Employee.FullNameEnglish ?? item.Employee.FullName,
            Department = isArabic ? item.Employee.Department.NameArabic ?? item.Employee.Department.Name : item.Employee.Department.Name,
            Branch = item.Employee.Branch == null ? null : isArabic ? item.Employee.Branch.NameArabic ?? item.Employee.Branch.Name : item.Employee.Branch.Name,
            DelegationType = isArabic ? item.DelegationType.NameArabic ?? item.DelegationType.Name : item.DelegationType.Name,
            item.Subject,
            item.AuthorizedEntity,
            item.StartDate,
            item.EndDate,
            item.Status,
            item.CreatedAt
        });
        return await PageAsync("Delegations Report", columns, projected, skip, take, maximumTotal, item => Row(
            ("delegationNumber", item.DelegationNumber), ("employeeNumber", item.EmployeeNumber),
            ("employeeName", item.EmployeeName), ("department", item.Department),
            ("delegationType", item.DelegationType), ("subject", item.Subject),
            ("authorizedEntity", item.AuthorizedEntity), ("startDate", item.StartDate),
            ("endDate", item.EndDate), ("status", item.Status), ("createdAt", item.CreatedAt)), cancellationToken);
    }

    private IQueryable<Employee> ApplyEmployeeFilters(IQueryable<Employee> query, HrReportFilterDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(item => EF.Functions.ILike(item.EmployeeNumber, pattern) ||
                                        EF.Functions.ILike(item.FullName, pattern) ||
                                        (item.FullNameArabic != null && EF.Functions.ILike(item.FullNameArabic, pattern)) ||
                                        (item.FullNameEnglish != null && EF.Functions.ILike(item.FullNameEnglish, pattern)));
        }
        if (filter.EmployeeId.HasValue) query = query.Where(item => item.Id == filter.EmployeeId.Value);
        if (filter.DepartmentId.HasValue) query = query.Where(item => item.DepartmentId == filter.DepartmentId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(item => EF.Functions.ILike(item.Status, status));
        }
        return query;
    }

    private IQueryable<AttendanceRecord> ApplyAttendanceFilters(
        IQueryable<AttendanceRecord> query,
        HrReportFilterDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(item => EF.Functions.ILike(item.Employee.EmployeeNumber, pattern) ||
                                        EF.Functions.ILike(item.Employee.FullName, pattern) ||
                                        (item.Employee.FullNameArabic != null && EF.Functions.ILike(item.Employee.FullNameArabic, pattern)) ||
                                        (item.Employee.FullNameEnglish != null && EF.Functions.ILike(item.Employee.FullNameEnglish, pattern)));
        }
        if (filter.EmployeeId.HasValue) query = query.Where(item => item.EmployeeId == filter.EmployeeId.Value);
        if (filter.DepartmentId.HasValue) query = query.Where(item => item.Employee.DepartmentId == filter.DepartmentId.Value);
        if (filter.DateFrom.HasValue) query = query.Where(item => item.AttendanceDate >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue) query = query.Where(item => item.AttendanceDate <= filter.DateTo.Value);
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = AttendanceValues.NormalizeStatus(filter.Status)
                ?? throw new HrValidationException("Attendance status is invalid.");
            query = query.Where(item => item.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            var source = AttendanceValues.NormalizeSource(filter.Type)
                ?? throw new HrValidationException("Attendance source is invalid.");
            query = query.Where(item => item.Source == source);
        }
        return query;
    }

    private static IQueryable<T> ApplyEmployeeRelationFilters<T>(
        IQueryable<T> query,
        HrReportFilterDto filter,
        System.Linq.Expressions.Expression<Func<T, Guid>> employeeId,
        System.Linq.Expressions.Expression<Func<T, Guid>> departmentId,
        System.Linq.Expressions.Expression<Func<T, Guid?>> branchId)
    {
        if (filter.EmployeeId.HasValue) query = query.Where(Equal(employeeId, filter.EmployeeId.Value));
        if (filter.DepartmentId.HasValue) query = query.Where(Equal(departmentId, filter.DepartmentId.Value));
        _ = branchId; // Kept in the internal signature while legacy branch data remains in the database.
        return query;
    }

    private static System.Linq.Expressions.Expression<Func<T, bool>> Equal<T, TValue>(
        System.Linq.Expressions.Expression<Func<T, TValue>> selector,
        TValue value)
    {
        var constant = System.Linq.Expressions.Expression.Constant(value);
        System.Linq.Expressions.Expression right = constant.Type == typeof(TValue)
            ? constant
            : System.Linq.Expressions.Expression.Convert(constant, typeof(TValue));
        var body = System.Linq.Expressions.Expression.Equal(selector.Body, right);
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, selector.Parameters);
    }

    private static async Task<ReportData> PageAsync<T>(
        string name,
        IReadOnlyCollection<HrReportColumnDto> columns,
        IQueryable<T> query,
        int skip,
        int take,
        int? maximumTotal,
        Func<T, HrReportRowDto> map,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        if (maximumTotal.HasValue && totalCount > maximumTotal.Value)
            throw new HrValidationException($"The export contains {totalCount} rows. Refine the filters to {maximumTotal.Value} rows or fewer.");
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
        return new ReportData(ApiTextLocalizer.Localize(name), columns, items.Select(map).ToArray(), totalCount);
    }

    private async Task<IReadOnlyDictionary<string, string>> BuildAppliedFiltersAsync(
        string reportCode,
        HrReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var values = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(filter.Search)) values[ApiTextLocalizer.Localize("Search")] = filter.Search.Trim();
        if (filter.DateFrom.HasValue) values[ApiTextLocalizer.Localize("Date From")] = filter.DateFrom.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (filter.DateTo.HasValue) values[ApiTextLocalizer.Localize("Date To")] = filter.DateTo.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (!string.IsNullOrWhiteSpace(filter.Status)) values[ApiTextLocalizer.Localize("Status")] = ApiTextLocalizer.LocalizeCode(filter.Status.Trim());
        if (!string.IsNullOrWhiteSpace(filter.Type)) values[ApiTextLocalizer.Localize("Type")] = filter.Type.Trim();
        if (filter.EmployeeId.HasValue)
        {
            values[ApiTextLocalizer.Localize("Employee")] = await _dbContext.Employees.AsNoTracking()
                .Where(item => item.Id == filter.EmployeeId.Value)
                .Select(item => item.EmployeeNumber + " - " + (isArabic ? item.FullNameArabic ?? item.FullName : item.FullNameEnglish ?? item.FullName))
                .SingleOrDefaultAsync(cancellationToken) ?? filter.EmployeeId.Value.ToString();
        }
        if (filter.DepartmentId.HasValue)
        {
            values[ApiTextLocalizer.Localize("Department")] = await _dbContext.Departments.AsNoTracking()
                .Where(item => item.Id == filter.DepartmentId.Value)
                .Select(item => isArabic ? item.NameArabic ?? item.Name : item.Name)
                .SingleOrDefaultAsync(cancellationToken) ?? filter.DepartmentId.Value.ToString();
        }
        if (filter.TypeId.HasValue)
        {
            values[ApiTextLocalizer.Localize("Type")] = await GetTypeNameAsync(reportCode, filter.TypeId.Value, cancellationToken)
                ?? filter.TypeId.Value.ToString();
        }
        return values;
    }

    private Task<string?> GetTypeNameAsync(string reportCode, Guid typeId, CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        return reportCode switch
        {
            HrReportCodes.Leave => _dbContext.LeaveTypes.AsNoTracking().Where(item => item.Id == typeId).Select(item => isArabic ? item.NameArabic ?? item.Name : item.Name).SingleOrDefaultAsync(cancellationToken),
            HrReportCodes.ExpiringContracts => _dbContext.ContractTypes.AsNoTracking().Where(item => item.Id == typeId).Select(item => isArabic ? item.NameArabic ?? item.Name : item.Name).SingleOrDefaultAsync(cancellationToken),
            HrReportCodes.ExpiringDocuments => _dbContext.DocumentTypes.AsNoTracking().Where(item => item.Id == typeId).Select(item => isArabic ? item.NameArabic ?? item.Name : item.Name).SingleOrDefaultAsync(cancellationToken),
            HrReportCodes.Delegations => _dbContext.DelegationTypes.AsNoTracking().Where(item => item.Id == typeId).Select(item => isArabic ? item.NameArabic ?? item.Name : item.Name).SingleOrDefaultAsync(cancellationToken),
            _ => Task.FromResult<string?>(null)
        };
    }

    private static HrReportCatalogItemDto GetDefinition(string code)
    {
        var normalized = code?.Trim().ToLowerInvariant();
        return Catalog.SingleOrDefault(item => item.Code == normalized)
            ?? throw new HrNotFoundException("Report was not found.");
    }

    private static void ValidateFilter(HrReportFilterDto filter)
    {
        if (filter.Page < 1 || filter.PageSize is < 1 or > MaximumPageSize)
            throw new HrValidationException("Invalid report pagination values.");
        if (filter.DateFrom.HasValue && filter.DateTo.HasValue)
        {
            if (filter.DateTo < filter.DateFrom) throw new HrValidationException("Date to cannot be before date from.");
            if (filter.DateTo.Value.DayNumber - filter.DateFrom.Value.DayNumber > MaximumDateRangeDays)
                throw new HrValidationException($"Report date ranges cannot exceed {MaximumDateRangeDays} days.");
        }
    }

    private static string? NormalizeAbsenceStatus(string value) => value.Trim().ToLowerInvariant() switch
    {
        "pending" => AbsenceValues.PendingStatus,
        "excused" => AbsenceValues.ExcusedStatus,
        "unexcused" => AbsenceValues.UnexcusedStatus,
        _ => null
    };

    private static string? NormalizeLeaveStatus(string value) => value.Trim().ToLowerInvariant() switch
    {
        "pending" => LeaveRequestStatuses.Pending,
        "approved" => LeaveRequestStatuses.Approved,
        "rejected" => LeaveRequestStatuses.Rejected,
        "cancelled" or "canceled" => LeaveRequestStatuses.Cancelled,
        _ => null
    };

    private static string? NormalizeContractStatus(string value) => value.Trim().ToLowerInvariant() switch
    {
        "draft" => EmployeeContract.DraftStatus,
        "active" => EmployeeContract.ActiveStatus,
        "expired" => EmployeeContract.ExpiredStatus,
        "terminated" => EmployeeContract.TerminatedStatus,
        _ => null
    };

    private static HrReportCatalogItemDto CatalogItem(
        string code,
        string name,
        string description,
        params string[] filters) => new(code, name, description, filters);

    private static IReadOnlyCollection<HrReportColumnDto> Columns(params (string Key, string Header)[] columns) =>
        columns.Select(item => new HrReportColumnDto(item.Key, ApiTextLocalizer.Localize(item.Header))).ToArray();

    private static HrReportRowDto Row(params (string Key, object? Value)[] values) =>
        new(values.ToDictionary(item => item.Key, item => Format(item.Value)));

    private static string? Format(object? value) => value switch
    {
        null => null,
        DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateTimeOffset timestamp => timestamp.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        decimal number => number.ToString("0.##", CultureInfo.InvariantCulture),
        double number => number.ToString("0.##", CultureInfo.InvariantCulture),
        bool boolean => ApiTextLocalizer.Localize(boolean ? "Yes" : "No"),
        string text => ApiTextLocalizer.LocalizeCode(text),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()
    };

    private static int Pages(int count, int size) => count == 0 ? 0 : (int)Math.Ceiling(count / (double)size);

    private sealed record ReportData(
        string Name,
        IReadOnlyCollection<HrReportColumnDto> Columns,
        IReadOnlyCollection<HrReportRowDto> Rows,
        int TotalCount);

    private enum AttendanceMode
    {
        All,
        Late,
        Overtime
    }
}
