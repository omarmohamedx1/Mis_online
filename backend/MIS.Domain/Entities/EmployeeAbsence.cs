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
    public decimal SuggestedDeductionAmount { get; private set; }
    public decimal? ApprovedDeductionAmount { get; private set; }
    public string PayrollImpactStatus { get; private set; } = AbsenceValues.PayrollNotApplicable;
    public string? PayrollNotes { get; private set; }
    public Guid? PayrollReviewedByUserId { get; private set; }
    public User? PayrollReviewedByUser { get; private set; }
    public DateTimeOffset? PayrollReviewedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(Guid employeeId, DateOnly absenceDate, string? reason, string status, string? notes, DateTimeOffset updatedAt)
    {
        if (PayrollImpactStatus == AbsenceValues.PayrollApproved &&
            (EmployeeId != employeeId || AbsenceDate != absenceDate || status != Status))
            throw new InvalidOperationException("An absence with an approved payroll deduction cannot change employee, date, or absence status.");
        SetDetails(employeeId, absenceDate, reason, status, notes);
        UpdatedAt = updatedAt;
    }

    public void SynchronizePayrollImpact(decimal? suggestedDeductionAmount, DateTimeOffset updatedAt)
    {
        if (PayrollImpactStatus == AbsenceValues.PayrollApproved) return;

        if (Status != AbsenceValues.UnexcusedStatus)
        {
            SuggestedDeductionAmount = 0;
            ApprovedDeductionAmount = null;
            PayrollImpactStatus = AbsenceValues.PayrollNotApplicable;
            PayrollNotes = null;
            PayrollReviewedByUserId = null;
            PayrollReviewedAt = null;
        }
        else
        {
            SuggestedDeductionAmount = decimal.Round(Math.Max(0, suggestedDeductionAmount ?? 0), 2, MidpointRounding.AwayFromZero);
            ApprovedDeductionAmount = null;
            PayrollImpactStatus = AbsenceValues.PayrollPendingReview;
            PayrollNotes = null;
            PayrollReviewedByUserId = null;
            PayrollReviewedAt = null;
        }
        UpdatedAt = updatedAt;
    }

    public void ReviewPayrollImpact(bool approve, decimal? approvedAmount, string? notes, Guid reviewedByUserId, DateTimeOffset reviewedAt)
    {
        if (Status != AbsenceValues.UnexcusedStatus)
            throw new InvalidOperationException("Only an unexcused absence can affect payroll.");
        if (reviewedByUserId == Guid.Empty) throw new ArgumentException("Reviewer is required.", nameof(reviewedByUserId));
        if (approve && (!approvedAmount.HasValue || approvedAmount.Value < 0))
            throw new ArgumentException("A non-negative approved deduction amount is required.", nameof(approvedAmount));

        ApprovedDeductionAmount = approve ? decimal.Round(approvedAmount!.Value, 2, MidpointRounding.AwayFromZero) : null;
        PayrollImpactStatus = approve ? AbsenceValues.PayrollApproved : AbsenceValues.PayrollExcluded;
        PayrollNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        PayrollReviewedByUserId = reviewedByUserId;
        PayrollReviewedAt = reviewedAt;
        UpdatedAt = reviewedAt;
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
