namespace MIS.Domain.Constants;

public static class LeaveRequestStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyCollection<string> All =
    [
        Pending,
        Approved,
        Rejected,
        Cancelled
    ];

    public static bool IsValid(string? status) => status is Pending or Approved or Rejected or Cancelled;
}
