using MIS.Domain.Constants;
using MIS.Domain.Entities;
using Xunit;

namespace MIS.Domain.Tests;

public sealed class EmployeeAbsencePayrollTests
{
    [Fact]
    public void Unexcused_absence_creates_a_reviewable_suggestion_without_automatic_deduction()
    {
        var now = DateTimeOffset.UtcNow;
        var absence = new EmployeeAbsence(Guid.NewGuid(), new DateOnly(2026, 8, 10), null, AbsenceValues.UnexcusedStatus, null, now);

        absence.SynchronizePayrollImpact(300m, now);

        Assert.Equal(AbsenceValues.PayrollPendingReview, absence.PayrollImpactStatus);
        Assert.Equal(300m, absence.SuggestedDeductionAmount);
        Assert.Null(absence.ApprovedDeductionAmount);
    }

    [Fact]
    public void Payroll_review_records_the_authorized_final_amount_and_reviewer()
    {
        var now = DateTimeOffset.UtcNow;
        var reviewerId = Guid.NewGuid();
        var absence = new EmployeeAbsence(Guid.NewGuid(), new DateOnly(2026, 8, 10), null, AbsenceValues.UnexcusedStatus, null, now);
        absence.SynchronizePayrollImpact(300m, now);

        absence.ReviewPayrollImpact(true, 275.125m, "Approved after HR review", reviewerId, now.AddMinutes(1));

        Assert.Equal(AbsenceValues.PayrollApproved, absence.PayrollImpactStatus);
        Assert.Equal(275.13m, absence.ApprovedDeductionAmount);
        Assert.Equal(reviewerId, absence.PayrollReviewedByUserId);
        Assert.Equal("Approved after HR review", absence.PayrollNotes);
    }

    [Fact]
    public void Excused_absence_can_never_be_approved_as_a_payroll_deduction()
    {
        var now = DateTimeOffset.UtcNow;
        var absence = new EmployeeAbsence(Guid.NewGuid(), new DateOnly(2026, 8, 10), null, AbsenceValues.ExcusedStatus, null, now);
        absence.SynchronizePayrollImpact(300m, now);

        Assert.Equal(AbsenceValues.PayrollNotApplicable, absence.PayrollImpactStatus);
        Assert.Throws<InvalidOperationException>(() =>
            absence.ReviewPayrollImpact(true, 300m, null, Guid.NewGuid(), now));
    }
}
