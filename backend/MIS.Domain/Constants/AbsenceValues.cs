namespace MIS.Domain.Constants;

public static class AbsenceValues
{
    public const string AbsentType = "Absent";
    public const string ManualSource = "Manual";
    public const string PendingStatus = "Pending";
    public const string ExcusedStatus = "Excused";
    public const string UnexcusedStatus = "Unexcused";
    public const string PayrollNotApplicable = "NotApplicable";
    public const string PayrollPendingReview = "PendingReview";
    public const string PayrollApproved = "Approved";
    public const string PayrollExcluded = "Excluded";

    public static bool IsValidStatus(string value) =>
        value is PendingStatus or ExcusedStatus or UnexcusedStatus;

    public static bool IsValidPayrollImpactStatus(string value) =>
        value is PayrollNotApplicable or PayrollPendingReview or PayrollApproved or PayrollExcluded;
}
