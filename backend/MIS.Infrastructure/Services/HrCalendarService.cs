using Microsoft.EntityFrameworkCore;
using MIS.Application.Common;
using MIS.Application.DTOs.Hr;
using MIS.Application.Interfaces;
using MIS.Domain.Constants;
using MIS.Domain.Entities;
using MIS.Infrastructure.Persistence;

namespace MIS.Infrastructure.Services;

public sealed class HrCalendarService : IHrCalendarService, IWorkingCalendarCalculator
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserContext _currentUser;
    private readonly IHrAuditService _audit;

    public HrCalendarService(ApplicationDbContext dbContext, ICurrentUserContext currentUser, IHrAuditService audit)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<WorkingCalendarDto> GetWorkingCalendarAsync(CancellationToken cancellationToken)
    {
        var calendar = await QueryCalendar().AsNoTracking().SingleOrDefaultAsync(cancellationToken)
            ?? throw new HrNotFoundException("The working calendar has not been configured.");
        return Map(calendar);
    }

    public async Task<WorkingCalendarDto> UpdateWorkingCalendarAsync(
        UpdateWorkingCalendarRequest request,
        CancellationToken cancellationToken)
    {
        ValidateTimeZone(request.TimeZoneId);
        ValidateDays(request.Days);
        var calendar = await QueryCalendar().SingleOrDefaultAsync(cancellationToken)
            ?? throw new HrNotFoundException("The working calendar has not been configured.");
        var oldValue = Map(calendar);
        var now = DateTimeOffset.UtcNow;
        calendar.Update(request.Name, request.TimeZoneId, now);
        foreach (var day in request.Days)
        {
            calendar.SetDay(
                day.DayOfWeek,
                day.IsWorkingDay,
                day.StartTime,
                day.EndTime,
                day.BreakMinutes,
                day.LateGraceMinutes,
                day.EarlyLeaveGraceMinutes,
                day.MinimumOvertimeMinutes,
                now);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updated = Map(calendar);
        await _audit.WriteAsync(new AuditWriteRequest(
            "WorkingCalendarUpdated",
            nameof(WorkingCalendar),
            calendar.Id.ToString(),
            null,
            oldValue,
            updated,
            "Updated company working calendar and weekend rules."), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return updated;
    }

    public async Task<PagedCalendarExceptionsDto> GetExceptionsAsync(
        CalendarExceptionFilterDto filter,
        CancellationToken cancellationToken)
    {
        ValidateDateFilter(filter.DateFrom, filter.DateTo);
        var query = _dbContext.CalendarExceptions.AsNoTracking().Where(item => !item.IsDeleted);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(item =>
                item.NameEnglish.ToLower().Contains(term) ||
                (item.NameArabic != null && item.NameArabic.ToLower().Contains(term)));
        }
        if (filter.DateFrom.HasValue) query = query.Where(item => item.Date >= filter.DateFrom);
        if (filter.DateTo.HasValue) query = query.Where(item => item.Date <= filter.DateTo);
        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            var type = CalendarValues.NormalizeExceptionType(filter.Type)
                ?? throw new HrValidationException("Calendar exception type is invalid.");
            query = query.Where(item => item.Type == type);
        }
        if (filter.IsActive.HasValue) query = query.Where(item => item.IsActive == filter.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(item => item.Date).ThenBy(item => item.NameEnglish)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(item => new CalendarExceptionListItemDto(
                item.Id, item.NameEnglish, item.NameArabic, item.Date, item.Type, item.OverrideMode, item.IsActive))
            .ToArrayAsync(cancellationToken);
        return new PagedCalendarExceptionsDto(items, total, filter.Page, filter.PageSize, Pages(total, filter.PageSize));
    }

    public async Task<CalendarExceptionDetailsDto> GetExceptionAsync(Guid exceptionId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.CalendarExceptions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == exceptionId && !item.IsDeleted, cancellationToken)
            ?? throw new HrNotFoundException("Calendar exception was not found.");
        return Map(entity);
    }

    public async Task<CalendarExceptionDetailsDto> CreateExceptionAsync(
        CreateCalendarExceptionRequest request,
        CancellationToken cancellationToken)
    {
        ValidateException(request.Type, request.OverrideMode, request.StartTime, request.EndTime, request.BreakMinutes);
        var calendar = await QueryCalendar().AsNoTracking().SingleOrDefaultAsync(cancellationToken)
            ?? throw new HrNotFoundException("The working calendar has not been configured.");
        await EnsureExceptionDateAvailableAsync(calendar.Id, request.Date, null, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var entity = new CalendarException(
            calendar.Id,
            request.NameEnglish,
            request.NameArabic,
            request.Date,
            request.Type,
            request.OverrideMode,
            request.StartTime,
            request.EndTime,
            request.BreakMinutes,
            request.Description,
            request.IsActive,
            _currentUser.UserId,
            now);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.CalendarExceptions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var created = Map(entity);
        await _audit.WriteAsync(new AuditWriteRequest(
            "CalendarExceptionCreated",
            nameof(CalendarException),
            entity.Id.ToString(),
            null,
            null,
            created,
            $"Created calendar exception for {request.Date:yyyy-MM-dd}."), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return created;
    }

    public async Task<CalendarExceptionDetailsDto> UpdateExceptionAsync(
        Guid exceptionId,
        UpdateCalendarExceptionRequest request,
        CancellationToken cancellationToken)
    {
        ValidateException(request.Type, request.OverrideMode, request.StartTime, request.EndTime, request.BreakMinutes);
        var entity = await GetTrackedExceptionAsync(exceptionId, cancellationToken);
        await EnsureExceptionDateAvailableAsync(entity.WorkingCalendarId, request.Date, exceptionId, cancellationToken);
        var oldValue = Map(entity);
        entity.Update(
            request.NameEnglish,
            request.NameArabic,
            request.Date,
            request.Type,
            request.OverrideMode,
            request.StartTime,
            request.EndTime,
            request.BreakMinutes,
            request.Description,
            request.IsActive,
            _currentUser.UserId,
            DateTimeOffset.UtcNow);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var updated = Map(entity);
        await _audit.WriteAsync(new AuditWriteRequest(
            "CalendarExceptionUpdated",
            nameof(CalendarException),
            entity.Id.ToString(),
            null,
            oldValue,
            updated,
            $"Updated calendar exception for {request.Date:yyyy-MM-dd}."), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return updated;
    }

    public async Task<CalendarExceptionDetailsDto> SetExceptionActiveAsync(
        Guid exceptionId,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var entity = await GetTrackedExceptionAsync(exceptionId, cancellationToken);
        var oldValue = new { entity.IsActive };
        entity.SetActive(isActive, _currentUser.UserId, DateTimeOffset.UtcNow);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _audit.WriteAsync(new AuditWriteRequest(
            "CalendarExceptionStatusChanged",
            nameof(CalendarException),
            entity.Id.ToString(),
            null,
            oldValue,
            new { entity.IsActive },
            isActive ? "Activated calendar exception." : "Deactivated calendar exception."), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Map(entity);
    }

    public async Task DeleteExceptionAsync(
        Guid exceptionId,
        DeleteCalendarExceptionRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await GetTrackedExceptionAsync(exceptionId, cancellationToken);
        var oldValue = Map(entity);
        entity.Delete(_currentUser.UserId, request.Reason, DateTimeOffset.UtcNow);
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _audit.WriteAsync(new AuditWriteRequest(
            "CalendarExceptionDeleted",
            nameof(CalendarException),
            entity.Id.ToString(),
            null,
            oldValue,
            null,
            request.Reason ?? "Soft-deleted calendar exception."), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<WorkDaySchedule> GetScheduleAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var calendar = await QueryCalendar().AsNoTracking().SingleOrDefaultAsync(cancellationToken)
            ?? throw new HrNotFoundException("The working calendar has not been configured.");
        var exception = await _dbContext.CalendarExceptions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.WorkingCalendarId == calendar.Id && item.Date == date && item.IsActive && !item.IsDeleted, cancellationToken);
        return BuildSchedule(calendar, exception, date);
    }

    public async Task<decimal> CountWorkingDaysAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        if (endDate < startDate) throw new HrValidationException("End date cannot be before start date.");
        if (endDate.DayNumber - startDate.DayNumber > 730) throw new HrValidationException("A leave period cannot exceed two years.");

        var calendar = await QueryCalendar().AsNoTracking().SingleOrDefaultAsync(cancellationToken)
            ?? throw new HrNotFoundException("The working calendar has not been configured.");
        var exceptions = await _dbContext.CalendarExceptions.AsNoTracking()
            .Where(item => item.WorkingCalendarId == calendar.Id && item.Date >= startDate && item.Date <= endDate && item.IsActive && !item.IsDeleted)
            .ToDictionaryAsync(item => item.Date, cancellationToken);

        decimal count = 0;
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            exceptions.TryGetValue(date, out var exception);
            if (BuildSchedule(calendar, exception, date).IsWorkingDay) count++;
        }
        return count;
    }

    public DateTimeOffset ToInstant(DateOnly date, TimeOnly time, string timeZoneId)
    {
        var zone = ValidateTimeZone(timeZoneId);
        var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(local)) throw new HrValidationException("The selected local time does not exist because of a daylight-saving transition.");
        var offset = zone.GetUtcOffset(local);
        return new DateTimeOffset(local, offset);
    }

    private IQueryable<WorkingCalendar> QueryCalendar() => _dbContext.WorkingCalendars.Include(item => item.Days);

    private static WorkDaySchedule BuildSchedule(WorkingCalendar calendar, CalendarException? exception, DateOnly date)
    {
        var baseDay = calendar.Days.Single(item => item.DayOfWeek == date.DayOfWeek);
        var template = baseDay.IsWorkingDay
            ? baseDay
            : calendar.Days.Where(item => item.IsWorkingDay).OrderBy(item => item.DayOfWeek).FirstOrDefault();

        if (exception is null)
        {
            return FromDay(date, baseDay, calendar.TimeZoneId, null, null);
        }

        if (exception.OverrideMode == CalendarValues.NonWorkingDayOverride)
        {
            return new WorkDaySchedule(date, false, null, null, 0, 0, 0, 0, calendar.TimeZoneId, exception.NameEnglish, exception.Type);
        }

        if (exception.OverrideMode == CalendarValues.CustomWorkingHoursOverride)
        {
            return new WorkDaySchedule(
                date,
                true,
                exception.StartTime,
                exception.EndTime,
                exception.BreakMinutes ?? 0,
                template?.LateGraceMinutes ?? 0,
                template?.EarlyLeaveGraceMinutes ?? 0,
                template?.MinimumOvertimeMinutes ?? 0,
                calendar.TimeZoneId,
                exception.NameEnglish,
                exception.Type);
        }

        if (template is null) throw new HrValidationException("At least one standard working day must be configured before adding a working-day override.");
        return FromDay(date, template, calendar.TimeZoneId, exception.NameEnglish, exception.Type);
    }

    private static WorkDaySchedule FromDay(
        DateOnly date,
        WorkingDaySetting day,
        string timeZoneId,
        string? exceptionName,
        string? exceptionType) => new(
        date,
        day.IsWorkingDay,
        day.StartTime,
        day.EndTime,
        day.BreakMinutes,
        day.LateGraceMinutes,
        day.EarlyLeaveGraceMinutes,
        day.MinimumOvertimeMinutes,
        timeZoneId,
        exceptionName,
        exceptionType);

    private async Task<CalendarException> GetTrackedExceptionAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.CalendarExceptions.SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken)
        ?? throw new HrNotFoundException("Calendar exception was not found.");

    private async Task EnsureExceptionDateAvailableAsync(Guid calendarId, DateOnly date, Guid? excludingId, CancellationToken cancellationToken)
    {
        if (await _dbContext.CalendarExceptions.AnyAsync(
                item => item.WorkingCalendarId == calendarId && item.Date == date && !item.IsDeleted && item.Id != excludingId,
                cancellationToken))
            throw new HrConflictException("Another calendar exception already exists for this date.");
    }

    private static void ValidateDays(IReadOnlyCollection<SaveWorkingDaySettingRequest> days)
    {
        if (days.Count != 7 || days.Select(item => item.DayOfWeek).Distinct().Count() != 7)
            throw new HrValidationException("The working calendar must define every day of the week exactly once.");
        if (!days.Any(item => item.IsWorkingDay))
            throw new HrValidationException("The working calendar must contain at least one working day.");

        foreach (var day in days)
        {
            if (day.IsWorkingDay && (!day.StartTime.HasValue || !day.EndTime.HasValue))
                throw new HrValidationException($"{day.DayOfWeek} requires start and end times.");
            if (!day.IsWorkingDay && (day.StartTime.HasValue || day.EndTime.HasValue || day.BreakMinutes != 0 ||
                                      day.LateGraceMinutes != 0 || day.EarlyLeaveGraceMinutes != 0 || day.MinimumOvertimeMinutes != 0))
                throw new HrValidationException($"{day.DayOfWeek} is non-working and cannot contain working-hour settings.");
        }
    }

    private static void ValidateException(string type, string mode, TimeOnly? start, TimeOnly? end, int? breakMinutes)
    {
        if (CalendarValues.NormalizeExceptionType(type) is null) throw new HrValidationException("Calendar exception type is invalid.");
        var normalizedMode = CalendarValues.NormalizeOverrideMode(mode)
            ?? throw new HrValidationException("Calendar override mode is invalid.");
        if (normalizedMode == CalendarValues.CustomWorkingHoursOverride && (!start.HasValue || !end.HasValue))
            throw new HrValidationException("Custom working hours require start and end times.");
        if (normalizedMode != CalendarValues.CustomWorkingHoursOverride && (start.HasValue || end.HasValue || breakMinutes.HasValue))
            throw new HrValidationException("Start, end, and break values are only valid for custom working hours.");
    }

    private static TimeZoneInfo ValidateTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new HrValidationException("The selected time zone is not available on this server.");
        }
        catch (InvalidTimeZoneException)
        {
            throw new HrValidationException("The selected time zone configuration is invalid.");
        }
    }

    private static void ValidateDateFilter(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && to < from) throw new HrValidationException("Date to cannot be before date from.");
    }

    private static WorkingCalendarDto Map(WorkingCalendar entity) => new(
        entity.Id,
        ApiTextLocalizer.Localize(entity.Name),
        entity.TimeZoneId,
        entity.Days.OrderBy(item => item.DayOfWeek).Select(item => new WorkingDaySettingDto(
            item.DayOfWeek,
            item.IsWorkingDay,
            item.StartTime,
            item.EndTime,
            item.BreakMinutes,
            item.LateGraceMinutes,
            item.EarlyLeaveGraceMinutes,
            item.MinimumOvertimeMinutes)).ToArray(),
        entity.CreatedAt,
        entity.UpdatedAt);

    private static CalendarExceptionDetailsDto Map(CalendarException entity) => new(
        entity.Id,
        entity.NameEnglish,
        entity.NameArabic,
        entity.Date,
        entity.Type,
        entity.OverrideMode,
        entity.StartTime,
        entity.EndTime,
        entity.BreakMinutes,
        entity.Description,
        entity.IsActive,
        entity.CreatedAt,
        entity.UpdatedAt);

    private static int Pages(int total, int pageSize) => total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
}
