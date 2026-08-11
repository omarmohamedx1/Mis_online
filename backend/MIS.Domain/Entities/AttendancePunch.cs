using MIS.Domain.Constants;

namespace MIS.Domain.Entities;

public sealed class AttendancePunch
{
    private AttendancePunch() { }

    public AttendancePunch(
        Guid attendanceRecordId,
        DateTimeOffset timestamp,
        string punchType,
        string source,
        int? sourceRowNumber,
        string? rawValue,
        string? rawDataJson,
        DateTimeOffset createdAt)
    {
        if (attendanceRecordId == Guid.Empty) throw new ArgumentException("Attendance record is required.", nameof(attendanceRecordId));
        if (timestamp == default) throw new ArgumentException("Punch timestamp is required.", nameof(timestamp));
        var normalizedType = AttendanceValues.NormalizePunchType(punchType)
            ?? throw new ArgumentException("Invalid punch type.", nameof(punchType));
        var normalizedSource = AttendanceValues.NormalizeSource(source)
            ?? throw new ArgumentException("Invalid attendance source.", nameof(source));
        if (sourceRowNumber <= 0) throw new ArgumentOutOfRangeException(nameof(sourceRowNumber));
        if (createdAt == default) throw new ArgumentException("Created timestamp is required.", nameof(createdAt));

        Id = Guid.NewGuid();
        AttendanceRecordId = attendanceRecordId;
        // Keep every persisted punch as an absolute UTC instant. Display-time conversion is
        // deliberately left to the API/UI boundary and the configured working calendar.
        Timestamp = timestamp.ToUniversalTime();
        PunchType = normalizedType;
        Source = normalizedSource;
        SourceRowNumber = sourceRowNumber;
        RawValue = NormalizeOptional(rawValue);
        RawDataJson = JsonText.NormalizeOptional(rawDataJson, nameof(rawDataJson));
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid AttendanceRecordId { get; private set; }
    public AttendanceRecord AttendanceRecord { get; private set; } = null!;
    public DateTimeOffset Timestamp { get; private set; }
    public string PunchType { get; private set; } = AttendanceValues.UnknownPunch;
    public string Source { get; private set; } = AttendanceValues.ManualSource;
    public int? SourceRowNumber { get; private set; }
    public string? RawValue { get; private set; }
    public string? RawDataJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
