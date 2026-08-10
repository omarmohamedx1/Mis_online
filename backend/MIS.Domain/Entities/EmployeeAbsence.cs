using MIS.Domain.Constants;

namespace MIS.Domain.Entities;

public sealed class EmployeeAbsence
{
    private EmployeeAbsence() { }

    public EmployeeAbsence(Guid employeeId, DateOnly absenceDate, string? reason, string status, string? notes, DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        Type = AbsenceValues.AbsentType;
        AttendanceSource = AbsenceValues.ManualSource;
        SetDetails(employeeId, absenceDate, reason, status, notes);
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public DateOnly AbsenceDate { get; private set; }
    public string Type { get; private set; } = AbsenceValues.AbsentType;
    public string? Reason { get; private set; }
    public string Status { get; private set; } = AbsenceValues.PendingStatus;
    public string? Notes { get; private set; }
    public string AttendanceSource { get; private set; } = AbsenceValues.ManualSource;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(Guid employeeId, DateOnly absenceDate, string? reason, string status, string? notes, DateTimeOffset updatedAt)
    {
        SetDetails(employeeId, absenceDate, reason, status, notes);
        UpdatedAt = updatedAt;
    }

    private void SetDetails(Guid employeeId, DateOnly absenceDate, string? reason, string status, string? notes)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (absenceDate == default) throw new ArgumentException("Absence date is required.", nameof(absenceDate));
        if (!AbsenceValues.IsValidStatus(status)) throw new ArgumentException("Invalid absence status.", nameof(status));
        EmployeeId = employeeId;
        AbsenceDate = absenceDate;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        Status = status;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
