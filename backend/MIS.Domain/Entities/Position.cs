namespace MIS.Domain.Entities;

public sealed class Position
{
    private Position() { }

    public Position(string name, string code, Guid? departmentId, DateTimeOffset createdAt)
        : this(name, code, null, null, departmentId, true, createdAt)
    {
    }

    public Position(string name, string code, string? nameArabic, string? description, Guid? departmentId, bool isActive, DateTimeOffset createdAt)
    {
        SetDetails(name, code, nameArabic, description, departmentId, isActive, createdAt);
        Id = Guid.NewGuid();
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? NameArabic { get; private set; }
    public string? Description { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public Department? Department { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(string name, string code, string? nameArabic, string? description, Guid? departmentId, bool isActive, DateTimeOffset updatedAt)
    {
        SetDetails(name, code, nameArabic, description, departmentId, isActive, updatedAt);
        UpdatedAt = updatedAt;
    }

    private void SetDetails(string name, string code, string? nameArabic, string? description, Guid? departmentId, bool isActive, DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        if (departmentId == Guid.Empty) throw new ArgumentException("Department identifier cannot be empty.", nameof(departmentId));
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));

        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        NameArabic = NormalizeOptional(nameArabic);
        Description = NormalizeOptional(description);
        DepartmentId = departmentId;
        IsActive = isActive;
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
