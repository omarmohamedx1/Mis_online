namespace MIS.Domain.Entities;

public sealed class WorkingDaySetting
{
    private WorkingDaySetting() { }

    public WorkingDaySetting(
        Guid workingCalendarId,
        DayOfWeek dayOfWeek,
        bool isWorkingDay,
        TimeOnly? startTime,
        TimeOnly? endTime,
        int breakMinutes,
        int lateGraceMinutes,
        int earlyLeaveGraceMinutes,
        int minimumOvertimeMinutes,
        DateTimeOffset createdAt)
    {
        if (workingCalendarId == Guid.Empty) throw new ArgumentException("Working calendar is required.", nameof(workingCalendarId));
        WorkingCalendarId = workingCalendarId;
        DayOfWeek = dayOfWeek;
        SetDetails(isWorkingDay, startTime, endTime, breakMinutes, lateGraceMinutes, earlyLeaveGraceMinutes, minimumOvertimeMinutes, createdAt);
        CreatedAt = createdAt;
    }

    public Guid WorkingCalendarId { get; private set; }
    public WorkingCalendar WorkingCalendar { get; private set; } = null!;
    public DayOfWeek DayOfWeek { get; private set; }
    public bool IsWorkingDay { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public int BreakMinutes { get; private set; }
    public int LateGraceMinutes { get; private set; }
    public int EarlyLeaveGraceMinutes { get; private set; }
    public int MinimumOvertimeMinutes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(
        bool isWorkingDay,
        TimeOnly? startTime,
        TimeOnly? endTime,
        int breakMinutes,
        int lateGraceMinutes,
        int earlyLeaveGraceMinutes,
        int minimumOvertimeMinutes,
        DateTimeOffset updatedAt)
    {
        SetDetails(isWorkingDay, startTime, endTime, breakMinutes, lateGraceMinutes, earlyLeaveGraceMinutes, minimumOvertimeMinutes, updatedAt);
        UpdatedAt = updatedAt;
    }

    private void SetDetails(
        bool isWorkingDay,
        TimeOnly? startTime,
        TimeOnly? endTime,
        int breakMinutes,
        int lateGraceMinutes,
        int earlyLeaveGraceMinutes,
        int minimumOvertimeMinutes,
        DateTimeOffset timestamp)
    {
        if (!Enum.IsDefined(DayOfWeek)) throw new ArgumentOutOfRangeException(nameof(DayOfWeek));
        if (isWorkingDay && (!startTime.HasValue || !endTime.HasValue)) throw new ArgumentException("Working days require start and end times.");
        if (!isWorkingDay && (startTime.HasValue || endTime.HasValue)) throw new ArgumentException("Non-working days cannot define working hours.");
        EnsureRange(breakMinutes, 0, 1440, nameof(breakMinutes));
        EnsureRange(lateGraceMinutes, 0, 240, nameof(lateGraceMinutes));
        EnsureRange(earlyLeaveGraceMinutes, 0, 240, nameof(earlyLeaveGraceMinutes));
        EnsureRange(minimumOvertimeMinutes, 0, 1440, nameof(minimumOvertimeMinutes));
        if (!isWorkingDay && (breakMinutes != 0 || lateGraceMinutes != 0 || earlyLeaveGraceMinutes != 0 || minimumOvertimeMinutes != 0))
            throw new ArgumentException("Non-working days cannot define break, grace, or overtime values.");
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));

        IsWorkingDay = isWorkingDay;
        StartTime = startTime;
        EndTime = endTime;
        BreakMinutes = breakMinutes;
        LateGraceMinutes = lateGraceMinutes;
        EarlyLeaveGraceMinutes = earlyLeaveGraceMinutes;
        MinimumOvertimeMinutes = minimumOvertimeMinutes;
    }

    private static void EnsureRange(int value, int minimum, int maximum, string parameterName)
    {
        if (value < minimum || value > maximum) throw new ArgumentOutOfRangeException(parameterName);
    }
}
