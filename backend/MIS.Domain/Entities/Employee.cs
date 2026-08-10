namespace MIS.Domain.Entities;

public sealed class Employee
{
    private Employee() { }

    public Employee(string employeeNumber, string fullName, Guid departmentId, bool isActive, DateTimeOffset createdAt)
    {
        SetDetails(employeeNumber, fullName, departmentId, isActive, createdAt);
        Id = Guid.NewGuid();
        CreatedAt = createdAt;
        UpdatedAt = null;
    }

    public Guid Id { get; private set; }
    public string EmployeeNumber { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public Guid DepartmentId { get; private set; }
    public Department Department { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(string employeeNumber, string fullName, Guid departmentId, bool isActive, DateTimeOffset updatedAt)
    {
        SetDetails(employeeNumber, fullName, departmentId, isActive, updatedAt);
        UpdatedAt = updatedAt;
    }

    private void SetDetails(string employeeNumber, string fullName, Guid departmentId, bool isActive, DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        if (departmentId == Guid.Empty) throw new ArgumentException("Department is required.", nameof(departmentId));
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));
        EmployeeNumber = employeeNumber.Trim();
        FullName = fullName.Trim();
        DepartmentId = departmentId;
        IsActive = isActive;
    }
}
