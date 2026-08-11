namespace MIS.Domain.Entities;

public sealed class EmploymentType
{
    private EmploymentType() { }

    public EmploymentType(string name, string code, DateTimeOffset createdAt)
        : this(name, code, null, null, true, createdAt)
    {
    }

    public EmploymentType(string name, string code, string? nameArabic, string? description, bool isActive, DateTimeOffset createdAt)
    {
        SetDetails(name, code, nameArabic, description, isActive, createdAt);
        Id = Guid.NewGuid();
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? NameArabic { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(string name, string code, string? nameArabic, string? description, bool isActive, DateTimeOffset updatedAt)
    {
        SetDetails(name, code, nameArabic, description, isActive, updatedAt);
        UpdatedAt = updatedAt;
    }

    private void SetDetails(string name, string code, string? nameArabic, string? description, bool isActive, DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));

        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        NameArabic = NormalizeOptional(nameArabic);
        Description = NormalizeOptional(description);
        IsActive = isActive;
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
