namespace MIS.Domain.Entities;

public sealed class LeaveType
{
    private LeaveType() { }

    public LeaveType(string name, string code, decimal defaultAnnualEntitlement, bool requiresAttachment, DateTimeOffset createdAt)
        : this(name, code, null, null, defaultAnnualEntitlement, requiresAttachment, true, createdAt)
    {
    }

    public LeaveType(
        string name,
        string code,
        string? nameArabic,
        string? description,
        decimal defaultAnnualEntitlement,
        bool requiresAttachment,
        bool isActive,
        DateTimeOffset createdAt)
    {
        SetDetails(name, code, nameArabic, description, defaultAnnualEntitlement, requiresAttachment, isActive, createdAt);
        Id = Guid.NewGuid();
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? NameArabic { get; private set; }
    public string? Description { get; private set; }
    public decimal DefaultAnnualEntitlement { get; private set; }
    public bool RequiresAttachment { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(string name, string code, string? nameArabic, string? description, decimal defaultAnnualEntitlement, bool requiresAttachment, bool isActive, DateTimeOffset updatedAt)
    {
        SetDetails(name, code, nameArabic, description, defaultAnnualEntitlement, requiresAttachment, isActive, updatedAt);
        UpdatedAt = updatedAt;
    }

    private void SetDetails(string name, string code, string? nameArabic, string? description, decimal defaultAnnualEntitlement, bool requiresAttachment, bool isActive, DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        if (defaultAnnualEntitlement < 0) throw new ArgumentOutOfRangeException(nameof(defaultAnnualEntitlement));
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));

        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        NameArabic = NormalizeOptional(nameArabic);
        Description = NormalizeOptional(description);
        DefaultAnnualEntitlement = defaultAnnualEntitlement;
        RequiresAttachment = requiresAttachment;
        IsActive = isActive;
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
