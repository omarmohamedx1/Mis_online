using MIS.Domain.Constants;

namespace MIS.Domain.Entities;

public sealed class CalendarException
{
    private CalendarException() { }

    public CalendarException(
        Guid workingCalendarId,
        string nameEnglish,
        string? nameArabic,
        DateOnly date,
        string type,
        string overrideMode,
        TimeOnly? startTime,
        TimeOnly? endTime,
        int? breakMinutes,
        string? description,
        bool isActive,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        WorkingCalendarId = EnsureRequiredId(workingCalendarId, nameof(workingCalendarId));
        CreatedByUserId = EnsureRequiredId(createdByUserId, nameof(createdByUserId));
        SetDetails(nameEnglish, nameArabic, date, type, overrideMode, startTime, endTime, breakMinutes, description, isActive, createdAt);
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid WorkingCalendarId { get; private set; }
    public WorkingCalendar WorkingCalendar { get; private set; } = null!;
    public string NameEnglish { get; private set; } = string.Empty;
    public string? NameArabic { get; private set; }
    public DateOnly Date { get; private set; }
    public string Type { get; private set; } = CalendarValues.OfficialHolidayType;
    public string OverrideMode { get; private set; } = CalendarValues.NonWorkingDayOverride;
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public int? BreakMinutes { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Guid CreatedByUserId { get; private set; }
    public User CreatedByUser { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public User? UpdatedByUser { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public Guid? DeletedByUserId { get; private set; }
    public User? DeletedByUser { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeleteReason { get; private set; }

    public void Update(
        string nameEnglish,
        string? nameArabic,
        DateOnly date,
        string type,
        string overrideMode,
        TimeOnly? startTime,
        TimeOnly? endTime,
        int? breakMinutes,
        string? description,
        bool isActive,
        Guid updatedByUserId,
        DateTimeOffset updatedAt)
    {
        EnsureNotDeleted();
        SetDetails(nameEnglish, nameArabic, date, type, overrideMode, startTime, endTime, breakMinutes, description, isActive, updatedAt);
        UpdatedByUserId = EnsureRequiredId(updatedByUserId, nameof(updatedByUserId));
        UpdatedAt = updatedAt;
    }

    public void SetActive(bool isActive, Guid updatedByUserId, DateTimeOffset updatedAt)
    {
        EnsureNotDeleted();
        if (updatedAt == default) throw new ArgumentException("Timestamp is required.", nameof(updatedAt));
        IsActive = isActive;
        UpdatedByUserId = EnsureRequiredId(updatedByUserId, nameof(updatedByUserId));
        UpdatedAt = updatedAt;
    }

    public void Delete(Guid deletedByUserId, string? reason, DateTimeOffset deletedAt)
    {
        EnsureNotDeleted();
        if (deletedAt == default) throw new ArgumentException("Timestamp is required.", nameof(deletedAt));
        IsDeleted = true;
        IsActive = false;
        DeletedByUserId = EnsureRequiredId(deletedByUserId, nameof(deletedByUserId));
        DeletedAt = deletedAt;
        DeleteReason = NormalizeOptional(reason);
    }

    private void SetDetails(
        string nameEnglish,
        string? nameArabic,
        DateOnly date,
        string type,
        string overrideMode,
        TimeOnly? startTime,
        TimeOnly? endTime,
        int? breakMinutes,
        string? description,
        bool isActive,
        DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameEnglish);
        if (date == default) throw new ArgumentException("Exception date is required.", nameof(date));
        var normalizedType = CalendarValues.NormalizeExceptionType(type)
            ?? throw new ArgumentException("Invalid calendar exception type.", nameof(type));
        var normalizedMode = CalendarValues.NormalizeOverrideMode(overrideMode)
            ?? throw new ArgumentException("Invalid calendar override mode.", nameof(overrideMode));
        if (normalizedMode == CalendarValues.CustomWorkingHoursOverride && (!startTime.HasValue || !endTime.HasValue))
            throw new ArgumentException("Custom working hours require start and end times.");
        if (breakMinutes is < 0 or > 1440) throw new ArgumentOutOfRangeException(nameof(breakMinutes));
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));

        NameEnglish = nameEnglish.Trim();
        NameArabic = NormalizeOptional(nameArabic);
        Date = date;
        Type = normalizedType;
        OverrideMode = normalizedMode;
        StartTime = normalizedMode == CalendarValues.CustomWorkingHoursOverride ? startTime : null;
        EndTime = normalizedMode == CalendarValues.CustomWorkingHoursOverride ? endTime : null;
        BreakMinutes = normalizedMode == CalendarValues.CustomWorkingHoursOverride ? breakMinutes ?? 0 : null;
        Description = NormalizeOptional(description);
        IsActive = isActive;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted) throw new InvalidOperationException("A deleted calendar exception cannot be changed.");
    }

    private static Guid EnsureRequiredId(Guid id, string parameterName) =>
        id == Guid.Empty ? throw new ArgumentException("Identifier is required.", parameterName) : id;

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
