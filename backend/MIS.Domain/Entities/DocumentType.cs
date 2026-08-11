namespace MIS.Domain.Entities;

public sealed class DocumentType
{
    private DocumentType() { }

    public DocumentType(string name, string code, bool requiresExpiryDate, DateTimeOffset createdAt)
        : this(name, code, null, null, requiresExpiryDate, true, createdAt)
    {
    }

    public DocumentType(string name, string code, string? nameArabic, string? description, bool requiresExpiryDate, bool isActive, DateTimeOffset createdAt)
    {
        SetDetails(name, code, nameArabic, description, requiresExpiryDate, isActive, createdAt);
        Id = Guid.NewGuid();
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? NameArabic { get; private set; }
    public string? Description { get; private set; }
    public bool RequiresExpiryDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(string name, string code, string? nameArabic, string? description, bool requiresExpiryDate, bool isActive, DateTimeOffset updatedAt)
    {
        SetDetails(name, code, nameArabic, description, requiresExpiryDate, isActive, updatedAt);
        UpdatedAt = updatedAt;
    }

    private void SetDetails(string name, string code, string? nameArabic, string? description, bool requiresExpiryDate, bool isActive, DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));

        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        NameArabic = NormalizeOptional(nameArabic);
        Description = NormalizeOptional(description);
        RequiresExpiryDate = requiresExpiryDate;
        IsActive = isActive;
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
