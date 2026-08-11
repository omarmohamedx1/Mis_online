namespace MIS.Domain.Entities;

public sealed class EmployeeEmergencyContact
{
    private EmployeeEmergencyContact() { }

    public EmployeeEmergencyContact(
        Guid employeeId,
        string contactName,
        string relationship,
        string mobileNumber,
        string? alternativeNumber,
        string? notes,
        bool isPrimary,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId == Guid.Empty
            ? throw new ArgumentException("Employee is required.", nameof(employeeId))
            : employeeId;
        SetDetails(contactName, relationship, mobileNumber, alternativeNumber, notes, isPrimary, createdAt);
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public string ContactName { get; private set; } = string.Empty;
    public string Relationship { get; private set; } = string.Empty;
    public string MobileNumber { get; private set; } = string.Empty;
    public string? AlternativeNumber { get; private set; }
    public string? Notes { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(
        string contactName,
        string relationship,
        string mobileNumber,
        string? alternativeNumber,
        string? notes,
        bool isPrimary,
        DateTimeOffset updatedAt)
    {
        SetDetails(contactName, relationship, mobileNumber, alternativeNumber, notes, isPrimary, updatedAt);
        UpdatedAt = updatedAt;
    }

    private void SetDetails(
        string contactName,
        string relationship,
        string mobileNumber,
        string? alternativeNumber,
        string? notes,
        bool isPrimary,
        DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contactName);
        ArgumentException.ThrowIfNullOrWhiteSpace(relationship);
        ArgumentException.ThrowIfNullOrWhiteSpace(mobileNumber);
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));

        ContactName = contactName.Trim();
        Relationship = relationship.Trim();
        MobileNumber = mobileNumber.Trim();
        AlternativeNumber = string.IsNullOrWhiteSpace(alternativeNumber) ? null : alternativeNumber.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        IsPrimary = isPrimary;
    }
}
