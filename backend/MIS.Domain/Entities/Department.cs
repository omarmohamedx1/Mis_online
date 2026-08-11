namespace MIS.Domain.Entities;

public sealed class Department
{
    private Department() { }

    public Department(string name, string code, DateTimeOffset createdAt)
        : this(name, code, null, null, true, createdAt)
    {
    }

    public Department(
        string name,
        string code,
        string? nameArabic,
        string? description,
        bool isActive,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        if (createdAt == default) throw new ArgumentException("Created timestamp is required.", nameof(createdAt));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        NameArabic = NormalizeOptional(nameArabic);
        Description = NormalizeOptional(description);
        IsActive = isActive;
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

    public void Update(
        string name,
        string code,
        string? nameArabic,
        string? description,
        bool isActive,
        DateTimeOffset updatedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        if (updatedAt == default) throw new ArgumentException("Updated timestamp is required.", nameof(updatedAt));

        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        NameArabic = NormalizeOptional(nameArabic);
        Description = NormalizeOptional(description);
        IsActive = isActive;
        UpdatedAt = updatedAt;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
