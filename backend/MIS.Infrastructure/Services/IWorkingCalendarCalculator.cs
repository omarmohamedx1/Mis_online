namespace MIS.Infrastructure.Services;

public sealed record WorkDaySchedule(
    DateOnly Date,
    bool IsWorkingDay,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int BreakMinutes,
    int LateGraceMinutes,
    int EarlyLeaveGraceMinutes,
    int MinimumOvertimeMinutes,
    string TimeZoneId,
    string? ExceptionName,
    string? ExceptionType);

public interface IWorkingCalendarCalculator
{
    Task<WorkDaySchedule> GetScheduleAsync(DateOnly date, CancellationToken cancellationToken);

    Task<decimal> CountWorkingDaysAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);

    DateTimeOffset ToInstant(DateOnly date, TimeOnly time, string timeZoneId);
}
