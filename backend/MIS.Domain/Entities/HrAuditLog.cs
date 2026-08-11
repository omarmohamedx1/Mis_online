using System.Text.Json;

namespace MIS.Domain.Entities;

public sealed class HrAuditLog
{
    private HrAuditLog() { }

    public HrAuditLog(
        Guid? userId,
        string action,
        string entityType,
        Guid entityId,
        Guid? employeeId,
        string? oldValue,
        string? newValue,
        string? description,
        DateTimeOffset timestamp)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User identifier cannot be empty.", nameof(userId));
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee identifier cannot be empty.", nameof(employeeId));
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        if (entityId == Guid.Empty) throw new ArgumentException("Entity identifier is required.", nameof(entityId));
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));

        Id = Guid.NewGuid();
        UserId = userId;
        Action = action.Trim();
        EntityType = entityType.Trim();
        EntityId = entityId;
        EmployeeId = employeeId;
        OldValue = NormalizeJson(oldValue, nameof(oldValue));
        NewValue = NormalizeJson(newValue, nameof(newValue));
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Timestamp = timestamp;
    }

    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }

    private static string? NormalizeJson(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        using var document = JsonDocument.Parse(value);
        return document.RootElement.GetRawText();
    }
}
