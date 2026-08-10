namespace MIS.Domain.Constants;

public static class AbsenceValues
{
    public const string AbsentType = "Absent";
    public const string ManualSource = "Manual";
    public const string PendingStatus = "Pending";
    public const string ExcusedStatus = "Excused";
    public const string UnexcusedStatus = "Unexcused";

    public static bool IsValidStatus(string value) =>
        value is PendingStatus or ExcusedStatus or UnexcusedStatus;
}
