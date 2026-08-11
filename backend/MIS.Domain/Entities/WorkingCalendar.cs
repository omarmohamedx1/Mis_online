namespace MIS.Domain.Entities;

public sealed class WorkingCalendar
{
    private readonly List<WorkingDaySetting> _days = [];

    private WorkingCalendar() { }

    public WorkingCalendar(string name, string timeZoneId, DateTimeOffset createdAt)
    {
        SetDetails(name, timeZoneId, createdAt);
        Id = Guid.NewGuid();
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string TimeZoneId { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public IReadOnlyCollection<WorkingDaySetting> Days => _days.AsReadOnly();

    public void Update(string name, string timeZoneId, DateTimeOffset updatedAt)
    {
        SetDetails(name, timeZoneId, updatedAt);
        UpdatedAt = updatedAt;
    }

    public WorkingDaySetting SetDay(
        DayOfWeek dayOfWeek,
        bool isWorkingDay,
        TimeOnly? startTime,
        TimeOnly? endTime,
        int breakMinutes,
        int lateGraceMinutes,
        int earlyLeaveGraceMinutes,
        int minimumOvertimeMinutes,
        DateTimeOffset timestamp)
    {
        var setting = _days.SingleOrDefault(x => x.DayOfWeek == dayOfWeek);
        if (setting is null)
        {
            setting = new WorkingDaySetting(
                Id,
                dayOfWeek,
                isWorkingDay,
                startTime,
                endTime,
                breakMinutes,
                lateGraceMinutes,
                earlyLeaveGraceMinutes,
                minimumOvertimeMinutes,
                timestamp);
            _days.Add(setting);
        }
        else
        {
            setting.Update(
                isWorkingDay,
                startTime,
                endTime,
                breakMinutes,
                lateGraceMinutes,
                earlyLeaveGraceMinutes,
                minimumOvertimeMinutes,
                timestamp);
        }

        UpdatedAt = timestamp;
        return setting;
    }

    private void SetDetails(string name, string timeZoneId, DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));

        Name = name.Trim();
        TimeZoneId = timeZoneId.Trim();
    }
}
