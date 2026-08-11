namespace MIS.Domain.Constants;

public static class DelegationStatuses
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyCollection<string> All = [Draft, Active, Expired, Cancelled];

    public static string Normalize(string value) => value.Trim().ToLowerInvariant() switch
    {
        "draft" => Draft,
        "active" => Active,
        "expired" => Expired,
        "cancelled" or "canceled" => Cancelled,
        _ => throw new ArgumentException("Delegation status is invalid.", nameof(value))
    };
}
