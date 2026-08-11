using Microsoft.EntityFrameworkCore;
using MIS.Application.Common;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;
using MIS.Domain.Constants;
using MIS.Domain.Entities;
using MIS.Infrastructure.Persistence;

namespace MIS.Infrastructure.Services;

public sealed class HrLeaveService : IHrLeaveService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IWorkingCalendarCalculator _calendar;
    private readonly ICurrentUserContext _currentUser;
    private readonly IHrAuditService _audit;

    public HrLeaveService(
        ApplicationDbContext dbContext,
        IWorkingCalendarCalculator calendar,
        ICurrentUserContext currentUser,
        IHrAuditService audit)
    {
        _dbContext = dbContext;
        _calendar = calendar;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<PagedLeaveRequestsDto> GetPagedAsync(
        LeaveRequestFilterDto filter,
        CancellationToken cancellationToken)
    {
        ValidatePage(filter.Page, filter.PageSize);
        ValidateDateRange(filter.DateFrom, filter.DateTo);
        var isArabic = ApiTextLocalizer.IsArabic;

        var query = _dbContext.LeaveRequests.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(item =>
                EF.Functions.ILike(item.Employee.EmployeeNumber, pattern) ||
                EF.Functions.ILike(item.Employee.FullName, pattern) ||
                (item.Employee.FullNameArabic != null && EF.Functions.ILike(item.Employee.FullNameArabic, pattern)) ||
                (item.Employee.FullNameEnglish != null && EF.Functions.ILike(item.Employee.FullNameEnglish, pattern)) ||
                EF.Functions.ILike(item.LeaveType.Name, pattern) ||
                (item.LeaveType.NameArabic != null && EF.Functions.ILike(item.LeaveType.NameArabic, pattern)));
        }

        if (filter.EmployeeId.HasValue) query = query.Where(item => item.EmployeeId == filter.EmployeeId.Value);
        if (filter.DepartmentId.HasValue) query = query.Where(item => item.Employee.DepartmentId == filter.DepartmentId.Value);
        if (filter.BranchId.HasValue) query = query.Where(item => item.Employee.BranchId == filter.BranchId.Value);
        if (filter.LeaveTypeId.HasValue) query = query.Where(item => item.LeaveTypeId == filter.LeaveTypeId.Value);
        if (filter.DateFrom.HasValue) query = query.Where(item => item.EndDate >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue) query = query.Where(item => item.StartDate <= filter.DateTo.Value);
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = NormalizeStatus(filter.Status)
                ?? throw new HrValidationException("Leave request status is invalid.");
            query = query.Where(item => item.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var records = await ApplySort(query, filter.SortBy, filter.SortDescending)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(item => new LeaveProjection(
                item.Id,
                item.EmployeeId,
                item.Employee.EmployeeNumber,
                isArabic ? item.Employee.FullNameArabic ?? item.Employee.FullName : item.Employee.FullNameEnglish ?? item.Employee.FullName,
                item.Employee.DepartmentId,
                isArabic ? item.Employee.Department.NameArabic ?? item.Employee.Department.Name : item.Employee.Department.Name,
                item.Employee.BranchId,
                item.Employee.Branch == null ? null : isArabic ? item.Employee.Branch.NameArabic ?? item.Employee.Branch.Name : item.Employee.Branch.Name,
                item.LeaveTypeId,
                isArabic ? item.LeaveType.NameArabic ?? item.LeaveType.Name : item.LeaveType.Name,
                item.StartDate,
                item.EndDate,
                item.NumberOfDays,
                item.Reason,
                item.Notes,
                item.AttachmentDocumentId,
                item.AttachmentDocument == null ? null : item.AttachmentDocument.FileName,
                item.RequestDate,
                item.Status,
                item.CreatedByUserId,
                item.CreatedByUser.Username,
                item.DecidedByUserId,
                item.DecidedByUser == null ? null : item.DecidedByUser.Username,
                item.DecidedAt,
                item.DecisionNotes,
                item.CreatedAt,
                item.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedLeaveRequestsDto(
            records.Select(MapListItem).ToArray(),
            totalCount,
            filter.Page,
            filter.PageSize,
            Pages(totalCount, filter.PageSize));
    }

    public async Task<LeaveRequestDetailsDto> GetDetailsAsync(
        Guid leaveRequestId,
        CancellationToken cancellationToken)
    {
        var projection = await QueryProjection(leaveRequestId).SingleOrDefaultAsync(cancellationToken)
            ?? throw new HrNotFoundException("Leave request was not found.");
        return MapDetails(projection);
    }

    public async Task<LeaveRequestDetailsDto> CreateAsync(
        CreateLeaveRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequiredDateRange(request.StartDate, request.EndDate);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        await EnsureRequestReferencesAsync(
            request.EmployeeId,
            request.LeaveTypeId,
            request.AttachmentDocumentId,
            cancellationToken);
        await EnsureNoOverlapAsync(request.EmployeeId, request.StartDate, request.EndDate, null, cancellationToken);
        var numberOfDays = await CountWorkingDaysAsync(request.StartDate, request.EndDate, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var entity = new LeaveRequest(
            request.EmployeeId,
            request.LeaveTypeId,
            request.StartDate,
            request.EndDate,
            numberOfDays,
            request.Reason,
            request.Notes,
            request.AttachmentDocumentId,
            now,
            _currentUser.UserId,
            now);

        _dbContext.LeaveRequests.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var created = await GetDetailsAsync(entity.Id, cancellationToken);
        await _audit.WriteAsync(new AuditWriteRequest(
            "LeaveCreated",
            nameof(LeaveRequest),
            entity.Id.ToString(),
            entity.EmployeeId,
            null,
            created,
            $"Created leave request for {entity.StartDate:yyyy-MM-dd} through {entity.EndDate:yyyy-MM-dd}."), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return created;
    }

    public async Task<LeaveRequestDetailsDto> UpdateAsync(
        Guid leaveRequestId,
        UpdateLeaveRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequiredDateRange(request.StartDate, request.EndDate);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var entity = await GetPendingTrackedAsync(leaveRequestId, cancellationToken);
        await EnsureRequestReferencesAsync(
            request.EmployeeId,
            request.LeaveTypeId,
            request.AttachmentDocumentId,
            cancellationToken);
        await EnsureNoOverlapAsync(request.EmployeeId, request.StartDate, request.EndDate, entity.Id, cancellationToken);
        var numberOfDays = await CountWorkingDaysAsync(request.StartDate, request.EndDate, cancellationToken);
        var oldValue = await GetDetailsAsync(entity.Id, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        entity.Update(
            request.EmployeeId,
            request.LeaveTypeId,
            request.StartDate,
            request.EndDate,
            numberOfDays,
            request.Reason,
            request.Notes,
            request.AttachmentDocumentId,
            now);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updated = await GetDetailsAsync(entity.Id, cancellationToken);
        await _audit.WriteAsync(new AuditWriteRequest(
            "LeaveUpdated",
            nameof(LeaveRequest),
            entity.Id.ToString(),
            entity.EmployeeId,
            oldValue,
            updated,
            "Updated pending leave request."), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return updated;
    }

    public async Task<LeaveRequestDetailsDto> ApproveAsync(
        Guid leaveRequestId,
        ApproveLeaveRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var entity = await GetPendingTrackedAsync(leaveRequestId, cancellationToken);
        await EnsureRequestReferencesAsync(
            entity.EmployeeId,
            entity.LeaveTypeId,
            entity.AttachmentDocumentId,
            cancellationToken);
        await EnsureNoOverlapAsync(entity.EmployeeId, entity.StartDate, entity.EndDate, entity.Id, cancellationToken);
        var recalculatedDays = await CountWorkingDaysAsync(entity.StartDate, entity.EndDate, cancellationToken);
        await EnsureNoRecordedAttendancePunchesAsync(entity, cancellationToken);
        var oldValue = await GetDetailsAsync(entity.Id, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (recalculatedDays != entity.NumberOfDays)
        {
            entity.Update(
                entity.EmployeeId,
                entity.LeaveTypeId,
                entity.StartDate,
                entity.EndDate,
                recalculatedDays,
                entity.Reason,
                entity.Notes,
                entity.AttachmentDocumentId,
                now);
        }
        entity.Approve(_currentUser.UserId, request.Notes, now);
        await ApplyApprovedLeaveToAttendanceAsync(entity, now, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updated = await GetDetailsAsync(entity.Id, cancellationToken);
        await _audit.WriteAsync(new AuditWriteRequest(
            "LeaveApproved",
            nameof(LeaveRequest),
            entity.Id.ToString(),
            entity.EmployeeId,
            oldValue,
            updated,
            "Approved leave request."), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return updated;
    }

    public Task<LeaveRequestDetailsDto> RejectAsync(
        Guid leaveRequestId,
        RejectLeaveRequest request,
        CancellationToken cancellationToken) =>
        DecideAsync(
            leaveRequestId,
            LeaveRequestStatuses.Rejected,
            request.Reason,
            "LeaveRejected",
            "Rejected leave request.",
            cancellationToken);

    public Task<LeaveRequestDetailsDto> CancelAsync(
        Guid leaveRequestId,
        CancelLeaveRequest request,
        CancellationToken cancellationToken) =>
        DecideAsync(
            leaveRequestId,
            LeaveRequestStatuses.Cancelled,
            request.Reason,
            "LeaveCancelled",
            "Cancelled leave request.",
            cancellationToken);

    public async Task<PagedLeaveBalancesDto> GetBalancesAsync(
        LeaveBalanceFilterDto filter,
        CancellationToken cancellationToken)
    {
        ValidatePage(filter.Page, filter.PageSize);
        ValidateYear(filter.Year);
        var query = BuildBalanceSeedQuery(
            filter.Year,
            filter.Search,
            filter.EmployeeId,
            filter.DepartmentId,
            filter.BranchId,
            filter.LeaveTypeId,
            activeEmployeesOnly: true);
        var totalCount = await query.CountAsync(cancellationToken);
        var seeds = await query
            .OrderBy(item => item.EmployeeNumber)
            .ThenBy(item => item.LeaveTypeName)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);
        var balances = await BuildBalancesAsync(seeds, filter.Year, cancellationToken);
        return new PagedLeaveBalancesDto(
            balances,
            totalCount,
            filter.Page,
            filter.PageSize,
            Pages(totalCount, filter.PageSize));
    }

    public async Task<IReadOnlyCollection<LeaveBalanceDto>> GetEmployeeBalancesAsync(
        Guid employeeId,
        int year,
        CancellationToken cancellationToken)
    {
        ValidateYear(year);
        if (employeeId == Guid.Empty || !await _dbContext.Employees.AnyAsync(item => item.Id == employeeId, cancellationToken))
            throw new HrNotFoundException("Employee was not found.");

        var seeds = await BuildBalanceSeedQuery(
                year,
                null,
                employeeId,
                null,
                null,
                null,
                activeEmployeesOnly: false)
            .OrderBy(item => item.LeaveTypeName)
            .ToListAsync(cancellationToken);
        return await BuildBalancesAsync(seeds, year, cancellationToken);
    }

    public async Task<LeaveEntitlementDto> UpsertEntitlementAsync(
        Guid employeeId,
        Guid leaveTypeId,
        int year,
        UpsertLeaveEntitlementRequest request,
        CancellationToken cancellationToken)
    {
        ValidateYear(year);
        if (request.BaseEntitlement is < 0 or > 10000 || request.Adjustment is < -10000 or > 10000)
            throw new HrValidationException("Leave entitlement values are outside the allowed range.");
        if (request.BaseEntitlement + request.Adjustment < 0)
            throw new HrValidationException("Total leave entitlement cannot be negative.");
        await EnsureEmployeeAndLeaveTypeActiveAsync(employeeId, leaveTypeId, cancellationToken);
        var entity = await _dbContext.EmployeeLeaveEntitlements.SingleOrDefaultAsync(
            item => item.EmployeeId == employeeId && item.LeaveTypeId == leaveTypeId && item.Year == year,
            cancellationToken);
        var oldValue = entity is null
            ? null
            : await GetEntitlementProjectionAsync(entity.Id, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var action = entity is null ? "LeaveEntitlementCreated" : "LeaveEntitlementUpdated";

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (entity is null)
        {
            entity = new EmployeeLeaveEntitlement(
                employeeId,
                leaveTypeId,
                year,
                request.BaseEntitlement,
                request.Adjustment,
                request.Notes,
                _currentUser.UserId,
                now);
            _dbContext.EmployeeLeaveEntitlements.Add(entity);
        }
        else
        {
            entity.Update(
                year,
                request.BaseEntitlement,
                request.Adjustment,
                request.Notes,
                _currentUser.UserId,
                now);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        var updated = await GetEntitlementProjectionAsync(entity.Id, cancellationToken);
        await _audit.WriteAsync(new AuditWriteRequest(
            action,
            nameof(EmployeeLeaveEntitlement),
            entity.Id.ToString(),
            employeeId,
            oldValue,
            updated,
            $"Set leave entitlement for {year}."), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return updated;
    }

    private async Task<LeaveRequestDetailsDto> DecideAsync(
        Guid leaveRequestId,
        string status,
        string reason,
        string auditAction,
        string description,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new HrValidationException("A rejection or cancellation reason is required.");
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var entity = status == LeaveRequestStatuses.Cancelled
            ? await GetCancellableTrackedAsync(leaveRequestId, cancellationToken)
            : await GetPendingTrackedAsync(leaveRequestId, cancellationToken);
        var wasApproved = entity.Status == LeaveRequestStatuses.Approved;
        var oldValue = await GetDetailsAsync(entity.Id, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (status == LeaveRequestStatuses.Rejected)
            entity.Reject(_currentUser.UserId, reason, now);
        else if (status == LeaveRequestStatuses.Cancelled)
            entity.Cancel(_currentUser.UserId, reason, now);
        else
            throw new HrValidationException("Leave decision status is invalid.");

        if (wasApproved) await RecalculateCancelledLeaveAttendanceAsync(entity, now, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updated = await GetDetailsAsync(entity.Id, cancellationToken);
        await _audit.WriteAsync(new AuditWriteRequest(
            auditAction,
            nameof(LeaveRequest),
            entity.Id.ToString(),
            entity.EmployeeId,
            oldValue,
            updated,
            description), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return updated;
    }

    private async Task<LeaveRequest> GetPendingTrackedAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.LeaveRequests.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new HrNotFoundException("Leave request was not found.");
        if (entity.Status != LeaveRequestStatuses.Pending)
            throw new HrConflictException($"A {entity.Status} leave request cannot be changed.");
        return entity;
    }

    private async Task<LeaveRequest> GetCancellableTrackedAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.LeaveRequests.SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new HrNotFoundException("Leave request was not found.");
        if (entity.Status is not (LeaveRequestStatuses.Pending or LeaveRequestStatuses.Approved))
            throw new HrConflictException($"A {entity.Status} leave request cannot be cancelled.");
        return entity;
    }

    private async Task EnsureRequestReferencesAsync(
        Guid employeeId,
        Guid leaveTypeId,
        Guid? attachmentDocumentId,
        CancellationToken cancellationToken)
    {
        var leaveType = await EnsureEmployeeAndLeaveTypeActiveAsync(employeeId, leaveTypeId, cancellationToken);
        if (leaveType.RequiresAttachment && !attachmentDocumentId.HasValue)
            throw new HrValidationException("The selected leave type requires an attachment.");
        if (attachmentDocumentId.HasValue && !await _dbContext.EmployeeDocuments.AnyAsync(
                item => item.Id == attachmentDocumentId.Value && item.EmployeeId == employeeId && !item.IsDeleted,
                cancellationToken))
            throw new HrValidationException("The selected attachment does not belong to this employee or is unavailable.");
    }

    private async Task<LeaveType> EnsureEmployeeAndLeaveTypeActiveAsync(
        Guid employeeId,
        Guid leaveTypeId,
        CancellationToken cancellationToken)
    {
        if (employeeId == Guid.Empty || !await _dbContext.Employees.AnyAsync(
                item => item.Id == employeeId && item.IsActive,
                cancellationToken))
            throw new HrValidationException("An active employee is required.");
        var leaveType = await _dbContext.LeaveTypes.AsNoTracking().SingleOrDefaultAsync(
            item => item.Id == leaveTypeId && item.IsActive,
            cancellationToken);
        return leaveType ?? throw new HrValidationException("An active leave type is required.");
    }

    private async Task EnsureNoOverlapAsync(
        Guid employeeId,
        DateOnly startDate,
        DateOnly endDate,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var overlaps = await _dbContext.LeaveRequests.AnyAsync(
            item => item.EmployeeId == employeeId &&
                    (!excludingId.HasValue || item.Id != excludingId.Value) &&
                    (item.Status == LeaveRequestStatuses.Pending || item.Status == LeaveRequestStatuses.Approved) &&
                    item.StartDate <= endDate && item.EndDate >= startDate,
            cancellationToken);
        if (overlaps) throw new HrConflictException("The employee already has an overlapping pending or approved leave request.");
    }

    private async Task<decimal> CountWorkingDaysAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var days = await _calendar.CountWorkingDaysAsync(startDate, endDate, cancellationToken);
        if (days <= 0) throw new HrValidationException("The selected period does not contain any working days.");
        return days;
    }

    private async Task ApplyApprovedLeaveToAttendanceAsync(
        LeaveRequest leave,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken)
    {
        var records = await _dbContext.AttendanceRecords
            .Where(item => item.EmployeeId == leave.EmployeeId && !item.IsDeleted &&
                           item.AttendanceDate >= leave.StartDate && item.AttendanceDate <= leave.EndDate &&
                           item.Status == AttendanceValues.AbsentStatus &&
                           item.CheckIn == null && item.CheckOut == null &&
                           !_dbContext.AttendancePunches.Any(punch => punch.AttendanceRecordId == item.Id))
            .ToListAsync(cancellationToken);
        foreach (var record in records)
        {
            record.UpdateSummary(
                record.EmployeeId,
                record.AttendanceDate,
                record.CheckIn,
                record.CheckOut,
                0,
                0,
                0,
                0,
                AttendanceValues.LeaveStatus,
                record.Notes,
                false,
                _currentUser.UserId,
                timestamp);
        }
    }

    private async Task EnsureNoRecordedAttendancePunchesAsync(
        LeaveRequest leave,
        CancellationToken cancellationToken)
    {
        var conflictingDates = await _dbContext.AttendanceRecords.AsNoTracking()
            .Where(item => item.EmployeeId == leave.EmployeeId && !item.IsDeleted &&
                           item.AttendanceDate >= leave.StartDate && item.AttendanceDate <= leave.EndDate &&
                           (item.CheckIn != null || item.CheckOut != null ||
                            _dbContext.AttendancePunches.Any(punch => punch.AttendanceRecordId == item.Id)))
            .OrderBy(item => item.AttendanceDate)
            .Select(item => item.AttendanceDate)
            .Take(5)
            .ToArrayAsync(cancellationToken);
        if (conflictingDates.Length == 0) return;

        var dates = string.Join(", ", conflictingDates.Select(item => item.ToString("yyyy-MM-dd")));
        throw new HrConflictException(
            $"Leave cannot be approved because recorded attendance punches exist on: {dates}. Resolve those attendance records first.");
    }

    private async Task RecalculateCancelledLeaveAttendanceAsync(
        LeaveRequest leave,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken)
    {
        var records = await _dbContext.AttendanceRecords
            .Where(item => item.EmployeeId == leave.EmployeeId && !item.IsDeleted &&
                           item.AttendanceDate >= leave.StartDate && item.AttendanceDate <= leave.EndDate &&
                           item.Status == AttendanceValues.LeaveStatus)
            .ToListAsync(cancellationToken);
        foreach (var record in records)
        {
            var schedule = await _calendar.GetScheduleAsync(record.AttendanceDate, cancellationToken);
            var calculation = CalculateAttendanceAfterLeaveCancellation(record, schedule);
            record.UpdateSummary(
                record.EmployeeId,
                record.AttendanceDate,
                record.CheckIn,
                record.CheckOut,
                calculation.WorkingMinutes,
                calculation.LateMinutes,
                calculation.EarlyLeaveMinutes,
                calculation.OvertimeMinutes,
                calculation.Status,
                record.Notes,
                false,
                _currentUser.UserId,
                timestamp);
        }
    }

    private AttendanceAfterLeaveCalculation CalculateAttendanceAfterLeaveCancellation(
        AttendanceRecord record,
        WorkDaySchedule schedule)
    {
        if (!schedule.IsWorkingDay)
        {
            var status = string.IsNullOrWhiteSpace(schedule.ExceptionType)
                ? AttendanceValues.WeekendStatus
                : AttendanceValues.HolidayStatus;
            return new AttendanceAfterLeaveCalculation(0, 0, 0, 0, status);
        }
        if (!record.CheckIn.HasValue && !record.CheckOut.HasValue)
            return new AttendanceAfterLeaveCalculation(0, 0, 0, 0, AttendanceValues.AbsentStatus);

        var working = record.CheckIn.HasValue && record.CheckOut.HasValue
            ? Math.Max(0, (int)Math.Floor((record.CheckOut.Value - record.CheckIn.Value).TotalMinutes) - schedule.BreakMinutes)
            : 0;
        var late = 0;
        var early = 0;
        var overtime = 0;
        if (schedule.StartTime.HasValue && schedule.EndTime.HasValue)
        {
            var plannedStart = _calendar.ToInstant(record.AttendanceDate, schedule.StartTime.Value, schedule.TimeZoneId);
            var endDate = schedule.EndTime.Value <= schedule.StartTime.Value ? record.AttendanceDate.AddDays(1) : record.AttendanceDate;
            var plannedEnd = _calendar.ToInstant(endDate, schedule.EndTime.Value, schedule.TimeZoneId);
            if (record.CheckIn > plannedStart.AddMinutes(schedule.LateGraceMinutes))
                late = Math.Max(0, (int)Math.Ceiling((record.CheckIn!.Value - plannedStart.AddMinutes(schedule.LateGraceMinutes)).TotalMinutes));
            if (record.CheckOut < plannedEnd.Subtract(TimeSpan.FromMinutes(schedule.EarlyLeaveGraceMinutes)))
                early = Math.Max(0, (int)Math.Ceiling((plannedEnd.Subtract(TimeSpan.FromMinutes(schedule.EarlyLeaveGraceMinutes)) - record.CheckOut!.Value).TotalMinutes));
            if (record.CheckOut > plannedEnd)
            {
                var candidate = Math.Max(0, (int)Math.Floor((record.CheckOut.Value - plannedEnd).TotalMinutes));
                overtime = candidate >= schedule.MinimumOvertimeMinutes ? candidate : 0;
            }
        }
        return new AttendanceAfterLeaveCalculation(
            working,
            late,
            early,
            overtime,
            late > 0 ? AttendanceValues.LateStatus : AttendanceValues.PresentStatus);
    }

    private IQueryable<LeaveProjection> QueryProjection(Guid id)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        return _dbContext.LeaveRequests.AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new LeaveProjection(
                item.Id,
                item.EmployeeId,
                item.Employee.EmployeeNumber,
                isArabic ? item.Employee.FullNameArabic ?? item.Employee.FullName : item.Employee.FullNameEnglish ?? item.Employee.FullName,
                item.Employee.DepartmentId,
                isArabic ? item.Employee.Department.NameArabic ?? item.Employee.Department.Name : item.Employee.Department.Name,
                item.Employee.BranchId,
                item.Employee.Branch == null ? null : isArabic ? item.Employee.Branch.NameArabic ?? item.Employee.Branch.Name : item.Employee.Branch.Name,
                item.LeaveTypeId,
                isArabic ? item.LeaveType.NameArabic ?? item.LeaveType.Name : item.LeaveType.Name,
                item.StartDate,
                item.EndDate,
                item.NumberOfDays,
                item.Reason,
                item.Notes,
                item.AttachmentDocumentId,
                item.AttachmentDocument == null ? null : item.AttachmentDocument.FileName,
                item.RequestDate,
                item.Status,
                item.CreatedByUserId,
                item.CreatedByUser.Username,
                item.DecidedByUserId,
                item.DecidedByUser == null ? null : item.DecidedByUser.Username,
                item.DecidedAt,
                item.DecisionNotes,
                item.CreatedAt,
                item.UpdatedAt));
    }

    private IQueryable<BalanceSeed> BuildBalanceSeedQuery(
        int year,
        string? search,
        Guid? employeeId,
        Guid? departmentId,
        Guid? branchId,
        Guid? leaveTypeId,
        bool activeEmployeesOnly)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        var employees = _dbContext.Employees.AsNoTracking().AsQueryable();
        if (activeEmployeesOnly) employees = employees.Where(item => item.IsActive);
        if (employeeId.HasValue) employees = employees.Where(item => item.Id == employeeId.Value);
        if (departmentId.HasValue) employees = employees.Where(item => item.DepartmentId == departmentId.Value);
        if (branchId.HasValue) employees = employees.Where(item => item.BranchId == branchId.Value);

        var leaveTypes = _dbContext.LeaveTypes.AsNoTracking().Where(item => item.IsActive);
        if (leaveTypeId.HasValue) leaveTypes = leaveTypes.Where(item => item.Id == leaveTypeId.Value);
        var entitlements = _dbContext.EmployeeLeaveEntitlements.AsNoTracking().Where(item => item.Year == year);
        IQueryable<BalanceSeed> query =
            from employee in employees
            from leaveType in leaveTypes
            join entitlement in entitlements
                on new { EmployeeId = employee.Id, LeaveTypeId = leaveType.Id }
                equals new { entitlement.EmployeeId, entitlement.LeaveTypeId }
                into employeeEntitlements
            from entitlement in employeeEntitlements.DefaultIfEmpty()
            select new BalanceSeed
            {
                EmployeeId = employee.Id,
                EmployeeNumber = employee.EmployeeNumber,
                EmployeeName = isArabic ? employee.FullNameArabic ?? employee.FullName : employee.FullNameEnglish ?? employee.FullName,
                LeaveTypeId = leaveType.Id,
                LeaveTypeName = isArabic ? leaveType.NameArabic ?? leaveType.Name : leaveType.Name,
                Entitled = entitlement == null
                    ? leaveType.DefaultAnnualEntitlement
                    : entitlement.BaseEntitlement + entitlement.Adjustment
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(item =>
                EF.Functions.ILike(item.EmployeeNumber, pattern) ||
                EF.Functions.ILike(item.EmployeeName, pattern) ||
                EF.Functions.ILike(item.LeaveTypeName, pattern));
        }

        return query;
    }

    private async Task<IReadOnlyCollection<LeaveBalanceDto>> BuildBalancesAsync(
        IReadOnlyCollection<BalanceSeed> seeds,
        int year,
        CancellationToken cancellationToken)
    {
        if (seeds.Count == 0) return [];
        var employeeIds = seeds.Select(item => item.EmployeeId).Distinct().ToArray();
        var leaveTypeIds = seeds.Select(item => item.LeaveTypeId).Distinct().ToArray();
        var pairs = seeds.Select(item => (item.EmployeeId, item.LeaveTypeId)).ToHashSet();
        var yearStart = new DateOnly(year, 1, 1);
        var yearEnd = new DateOnly(year, 12, 31);
        var requests = await _dbContext.LeaveRequests.AsNoTracking()
            .Where(item =>
                employeeIds.Contains(item.EmployeeId) &&
                leaveTypeIds.Contains(item.LeaveTypeId) &&
                (item.Status == LeaveRequestStatuses.Approved || item.Status == LeaveRequestStatuses.Pending) &&
                item.StartDate <= yearEnd && item.EndDate >= yearStart)
            .Select(item => new BalanceRequest(
                item.EmployeeId,
                item.LeaveTypeId,
                item.StartDate,
                item.EndDate,
                item.NumberOfDays,
                item.Status))
            .ToListAsync(cancellationToken);

        var totals = new Dictionary<(Guid EmployeeId, Guid LeaveTypeId), BalanceTotals>();
        foreach (var request in requests.Where(item => pairs.Contains((item.EmployeeId, item.LeaveTypeId))))
        {
            var days = request.StartDate >= yearStart && request.EndDate <= yearEnd
                ? request.NumberOfDays
                : await _calendar.CountWorkingDaysAsync(
                    request.StartDate < yearStart ? yearStart : request.StartDate,
                    request.EndDate > yearEnd ? yearEnd : request.EndDate,
                    cancellationToken);
            var key = (request.EmployeeId, request.LeaveTypeId);
            var total = totals.GetValueOrDefault(key) ?? new BalanceTotals();
            if (request.Status == LeaveRequestStatuses.Approved) total.Used += days;
            else total.Pending += days;
            totals[key] = total;
        }

        var asOfDate = DateOnly.FromDateTime(DateTime.UtcNow);
        return seeds.Select(seed =>
        {
            var total = totals.GetValueOrDefault((seed.EmployeeId, seed.LeaveTypeId)) ?? new BalanceTotals();
            return new LeaveBalanceDto(
                seed.EmployeeId,
                seed.EmployeeNumber,
                seed.EmployeeName,
                seed.LeaveTypeId,
                seed.LeaveTypeName,
                year,
                seed.Entitled,
                total.Used,
                total.Pending,
                seed.Entitled - total.Used - total.Pending,
                asOfDate);
        }).ToArray();
    }

    private async Task<LeaveEntitlementDto> GetEntitlementProjectionAsync(
        Guid entitlementId,
        CancellationToken cancellationToken)
    {
        var isArabic = ApiTextLocalizer.IsArabic;
        return await _dbContext.EmployeeLeaveEntitlements.AsNoTracking()
            .Where(item => item.Id == entitlementId)
            .Select(item => new LeaveEntitlementDto(
                item.Id,
                item.EmployeeId,
                item.Employee.EmployeeNumber,
                isArabic ? item.Employee.FullNameArabic ?? item.Employee.FullName : item.Employee.FullNameEnglish ?? item.Employee.FullName,
                item.LeaveTypeId,
                isArabic ? item.LeaveType.NameArabic ?? item.LeaveType.Name : item.LeaveType.Name,
                item.Year,
                item.BaseEntitlement,
                item.Adjustment,
                item.BaseEntitlement + item.Adjustment,
                item.Notes,
                item.CreatedByUserId,
                item.CreatedByUser.Username,
                item.UpdatedByUserId,
                item.UpdatedByUser == null ? null : item.UpdatedByUser.Username,
                item.CreatedAt,
                item.UpdatedAt))
            .SingleAsync(cancellationToken);
    }

    private static IQueryable<LeaveRequest> ApplySort(
        IQueryable<LeaveRequest> query,
        string? sortBy,
        bool descending)
    {
        var key = sortBy?.Trim().ToLowerInvariant();
        return (key, descending) switch
        {
            ("employeenumber", false) => query.OrderBy(item => item.Employee.EmployeeNumber).ThenByDescending(item => item.RequestDate),
            ("employeenumber", true) => query.OrderByDescending(item => item.Employee.EmployeeNumber).ThenByDescending(item => item.RequestDate),
            ("employeename", false) => query.OrderBy(item => item.Employee.FullName).ThenByDescending(item => item.RequestDate),
            ("employeename", true) => query.OrderByDescending(item => item.Employee.FullName).ThenByDescending(item => item.RequestDate),
            ("leavetype", false) => query.OrderBy(item => item.LeaveType.Name).ThenByDescending(item => item.RequestDate),
            ("leavetype", true) => query.OrderByDescending(item => item.LeaveType.Name).ThenByDescending(item => item.RequestDate),
            ("startdate", false) => query.OrderBy(item => item.StartDate).ThenBy(item => item.Employee.EmployeeNumber),
            ("startdate", true) => query.OrderByDescending(item => item.StartDate).ThenBy(item => item.Employee.EmployeeNumber),
            ("enddate", false) => query.OrderBy(item => item.EndDate).ThenBy(item => item.Employee.EmployeeNumber),
            ("enddate", true) => query.OrderByDescending(item => item.EndDate).ThenBy(item => item.Employee.EmployeeNumber),
            ("numberofdays", false) => query.OrderBy(item => item.NumberOfDays).ThenByDescending(item => item.RequestDate),
            ("numberofdays", true) => query.OrderByDescending(item => item.NumberOfDays).ThenByDescending(item => item.RequestDate),
            ("status", false) => query.OrderBy(item => item.Status).ThenByDescending(item => item.RequestDate),
            ("status", true) => query.OrderByDescending(item => item.Status).ThenByDescending(item => item.RequestDate),
            (null or "" or "requestdate", false) => query.OrderBy(item => item.RequestDate).ThenBy(item => item.Id),
            (null or "" or "requestdate", true) => query.OrderByDescending(item => item.RequestDate).ThenByDescending(item => item.Id),
            _ => throw new HrValidationException("Leave request sort field is invalid.")
        };
    }

    private static LeaveRequestListItemDto MapListItem(LeaveProjection item) => new(
        item.Id,
        item.EmployeeId,
        item.EmployeeNumber,
        item.EmployeeName,
        item.DepartmentId,
        item.DepartmentName,
        item.BranchId,
        item.BranchName,
        item.LeaveTypeId,
        item.LeaveTypeName,
        item.StartDate,
        item.EndDate,
        item.NumberOfDays,
        item.RequestDate,
        item.Status);

    private static LeaveRequestDetailsDto MapDetails(LeaveProjection item) => new(
        item.Id,
        item.EmployeeId,
        item.EmployeeNumber,
        item.EmployeeName,
        item.DepartmentId,
        item.DepartmentName,
        item.BranchId,
        item.BranchName,
        item.LeaveTypeId,
        item.LeaveTypeName,
        item.StartDate,
        item.EndDate,
        item.NumberOfDays,
        item.Reason,
        item.Notes,
        item.AttachmentDocumentId,
        item.AttachmentFileName,
        item.RequestDate,
        item.Status,
        item.CreatedByUserId,
        item.CreatedByUsername,
        item.DecidedByUserId,
        item.DecidedByUsername,
        item.DecidedAt,
        item.DecisionNotes,
        item.CreatedAt,
        item.UpdatedAt);

    private static string? NormalizeStatus(string? status) => status?.Trim().ToLowerInvariant() switch
    {
        "pending" => LeaveRequestStatuses.Pending,
        "approved" => LeaveRequestStatuses.Approved,
        "rejected" => LeaveRequestStatuses.Rejected,
        "cancelled" or "canceled" => LeaveRequestStatuses.Cancelled,
        _ => null
    };

    private static void ValidatePage(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 200) throw new HrValidationException("Invalid pagination values.");
    }

    private static void ValidateDateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && to < from) throw new HrValidationException("Date to cannot be before date from.");
    }

    private static void ValidateRequiredDateRange(DateOnly from, DateOnly to)
    {
        if (from == default || to == default) throw new HrValidationException("Leave start and end dates are required.");
        if (to < from) throw new HrValidationException("Leave end date cannot be before start date.");
    }

    private static void ValidateYear(int year)
    {
        if (year is < 2000 or > 9999) throw new HrValidationException("Leave balance year is invalid.");
    }

    private static int Pages(int count, int size) => count == 0 ? 0 : (int)Math.Ceiling(count / (double)size);

    private sealed record LeaveProjection(
        Guid Id,
        Guid EmployeeId,
        string EmployeeNumber,
        string EmployeeName,
        Guid DepartmentId,
        string DepartmentName,
        Guid? BranchId,
        string? BranchName,
        Guid LeaveTypeId,
        string LeaveTypeName,
        DateOnly StartDate,
        DateOnly EndDate,
        decimal NumberOfDays,
        string? Reason,
        string? Notes,
        Guid? AttachmentDocumentId,
        string? AttachmentFileName,
        DateTimeOffset RequestDate,
        string Status,
        Guid CreatedByUserId,
        string CreatedByUsername,
        Guid? DecidedByUserId,
        string? DecidedByUsername,
        DateTimeOffset? DecidedAt,
        string? DecisionNotes,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    private sealed class BalanceSeed
    {
        public Guid EmployeeId { get; init; }
        public string EmployeeNumber { get; init; } = string.Empty;
        public string EmployeeName { get; init; } = string.Empty;
        public Guid LeaveTypeId { get; init; }
        public string LeaveTypeName { get; init; } = string.Empty;
        public decimal Entitled { get; init; }
    }

    private sealed record BalanceRequest(
        Guid EmployeeId,
        Guid LeaveTypeId,
        DateOnly StartDate,
        DateOnly EndDate,
        decimal NumberOfDays,
        string Status);

    private sealed class BalanceTotals
    {
        public decimal Used { get; set; }
        public decimal Pending { get; set; }
    }

    private sealed record AttendanceAfterLeaveCalculation(
        int WorkingMinutes,
        int LateMinutes,
        int EarlyLeaveMinutes,
        int OvertimeMinutes,
        string Status);
}
