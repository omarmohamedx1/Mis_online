using MIS.Domain.Constants;

namespace MIS.Domain.Entities;

public sealed class AttendanceRecord
{
    private AttendanceRecord() { }

    public AttendanceRecord(
        Guid employeeId,
        DateOnly attendanceDate,
        DateTimeOffset? checkIn,
        DateTimeOffset? checkOut,
        int workingMinutes,
        int lateMinutes,
        int earlyLeaveMinutes,
        int overtimeMinutes,
        string status,
        string source,
        string? notes,
        Guid? importBatchId,
        bool isManuallyAdjusted,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        CreatedByUserId = EnsureRequiredId(createdByUserId, nameof(createdByUserId));
        SetSummary(
            employeeId,
            attendanceDate,
            checkIn,
            checkOut,
            workingMinutes,
            lateMinutes,
            earlyLeaveMinutes,
            overtimeMinutes,
            status,
            source,
            notes,
            importBatchId,
            isManuallyAdjusted,
            createdAt);
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public DateOnly AttendanceDate { get; private set; }
    public DateTimeOffset? CheckIn { get; private set; }
    public DateTimeOffset? CheckOut { get; private set; }
    public int WorkingMinutes { get; private set; }
    public decimal WorkingHours => decimal.Round(WorkingMinutes / 60m, 2, MidpointRounding.AwayFromZero);
    public int LateMinutes { get; private set; }
    public int EarlyLeaveMinutes { get; private set; }
    public int OvertimeMinutes { get; private set; }
    public string Status { get; private set; } = AttendanceValues.PresentStatus;
    public string Source { get; private set; } = AttendanceValues.ManualSource;
    public string? Notes { get; private set; }
    public Guid? ImportBatchId { get; private set; }
    public AttendanceImportBatch? ImportBatch { get; private set; }
    public bool IsManuallyAdjusted { get; private set; }
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

    public void UpdateSummary(
        Guid employeeId,
        DateOnly attendanceDate,
        DateTimeOffset? checkIn,
        DateTimeOffset? checkOut,
        int workingMinutes,
        int lateMinutes,
        int earlyLeaveMinutes,
        int overtimeMinutes,
        string status,
        string? notes,
        bool markManuallyAdjusted,
        Guid updatedByUserId,
        DateTimeOffset updatedAt)
    {
        EnsureNotDeleted();
        SetSummary(
            employeeId,
            attendanceDate,
            checkIn,
            checkOut,
            workingMinutes,
            lateMinutes,
            earlyLeaveMinutes,
            overtimeMinutes,
            status,
            Source,
            notes,
            ImportBatchId,
            IsManuallyAdjusted || markManuallyAdjusted,
            updatedAt);
        UpdatedByUserId = EnsureRequiredId(updatedByUserId, nameof(updatedByUserId));
        UpdatedAt = updatedAt;
    }

    public void Delete(Guid deletedByUserId, string? reason, DateTimeOffset deletedAt)
    {
        EnsureNotDeleted();
        if (deletedAt == default) throw new ArgumentException("Timestamp is required.", nameof(deletedAt));
        IsDeleted = true;
        DeletedByUserId = EnsureRequiredId(deletedByUserId, nameof(deletedByUserId));
        DeletedAt = deletedAt;
        DeleteReason = NormalizeOptional(reason);
    }

    private void SetSummary(
        Guid employeeId,
        DateOnly attendanceDate,
        DateTimeOffset? checkIn,
        DateTimeOffset? checkOut,
        int workingMinutes,
        int lateMinutes,
        int earlyLeaveMinutes,
        int overtimeMinutes,
        string status,
        string source,
        string? notes,
        Guid? importBatchId,
        bool isManuallyAdjusted,
        DateTimeOffset timestamp)
    {
        EmployeeId = EnsureRequiredId(employeeId, nameof(employeeId));
        if (attendanceDate == default) throw new ArgumentException("Attendance date is required.", nameof(attendanceDate));
        if (checkOut < checkIn) throw new ArgumentException("Check-out cannot be before check-in.", nameof(checkOut));
        EnsureNonNegative(workingMinutes, nameof(workingMinutes));
        EnsureNonNegative(lateMinutes, nameof(lateMinutes));
        EnsureNonNegative(earlyLeaveMinutes, nameof(earlyLeaveMinutes));
        EnsureNonNegative(overtimeMinutes, nameof(overtimeMinutes));
        var normalizedStatus = AttendanceValues.NormalizeStatus(status)
            ?? throw new ArgumentException("Invalid attendance status.", nameof(status));
        var normalizedSource = AttendanceValues.NormalizeSource(source)
            ?? throw new ArgumentException("Invalid attendance source.", nameof(source));
        if (importBatchId == Guid.Empty) throw new ArgumentException("Import batch identifier cannot be empty.", nameof(importBatchId));
        if (normalizedSource == AttendanceValues.ExcelImportSource && !importBatchId.HasValue)
            throw new ArgumentException("Excel-imported attendance requires an import batch.", nameof(importBatchId));
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));

        AttendanceDate = attendanceDate;
        // PostgreSQL timestamptz represents instants and Npgsql requires DateTimeOffset values
        // to have a zero offset. Preserve the instant while enforcing that persistence invariant.
        CheckIn = checkIn?.ToUniversalTime();
        CheckOut = checkOut?.ToUniversalTime();
        WorkingMinutes = workingMinutes;
        LateMinutes = lateMinutes;
        EarlyLeaveMinutes = earlyLeaveMinutes;
        OvertimeMinutes = overtimeMinutes;
        Status = normalizedStatus;
        Source = normalizedSource;
        Notes = NormalizeOptional(notes);
        ImportBatchId = importBatchId;
        IsManuallyAdjusted = isManuallyAdjusted;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted) throw new InvalidOperationException("A deleted attendance record cannot be changed.");
    }

    private static void EnsureNonNegative(int value, string parameterName)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(parameterName);
    }

    private static Guid EnsureRequiredId(Guid id, string parameterName) =>
        id == Guid.Empty ? throw new ArgumentException("Identifier is required.", parameterName) : id;

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
