namespace MIS.Domain.Entities;

public sealed class EmployeeLeaveEntitlement
{
    private EmployeeLeaveEntitlement() { }

    public EmployeeLeaveEntitlement(
        Guid employeeId,
        Guid leaveTypeId,
        int year,
        decimal baseEntitlement,
        decimal adjustment,
        string? notes,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        EmployeeId = EnsureRequiredId(employeeId, nameof(employeeId));
        LeaveTypeId = EnsureRequiredId(leaveTypeId, nameof(leaveTypeId));
        CreatedByUserId = EnsureRequiredId(createdByUserId, nameof(createdByUserId));
        SetDetails(year, baseEntitlement, adjustment, notes);
        EnsureTimestamp(createdAt, nameof(createdAt));
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public Guid LeaveTypeId { get; private set; }
    public LeaveType LeaveType { get; private set; } = null!;
    public int Year { get; private set; }
    public decimal BaseEntitlement { get; private set; }
    public decimal Adjustment { get; private set; }
    public string? Notes { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public User CreatedByUser { get; private set; } = null!;
    public Guid? UpdatedByUserId { get; private set; }
    public User? UpdatedByUser { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(
        int year,
        decimal baseEntitlement,
        decimal adjustment,
        string? notes,
        Guid updatedByUserId,
        DateTimeOffset updatedAt)
    {
        var validatedUpdatedByUserId = EnsureRequiredId(updatedByUserId, nameof(updatedByUserId));
        EnsureTimestamp(updatedAt, nameof(updatedAt));
        if (updatedAt < CreatedAt)
            throw new ArgumentException("Update timestamp cannot be before creation.", nameof(updatedAt));

        SetDetails(year, baseEntitlement, adjustment, notes);
        UpdatedByUserId = validatedUpdatedByUserId;
        UpdatedAt = updatedAt;
    }

    private void SetDetails(int year, decimal baseEntitlement, decimal adjustment, string? notes)
    {
        if (year is < 1900 or > 9999) throw new ArgumentOutOfRangeException(nameof(year));
        if (baseEntitlement < 0) throw new ArgumentOutOfRangeException(nameof(baseEntitlement));
        if (baseEntitlement + adjustment < 0)
            throw new ArgumentOutOfRangeException(nameof(adjustment), "Total entitlement cannot be negative.");

        Year = year;
        BaseEntitlement = baseEntitlement;
        Adjustment = adjustment;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    private static Guid EnsureRequiredId(Guid id, string parameterName) =>
        id == Guid.Empty ? throw new ArgumentException("Identifier is required.", parameterName) : id;

    private static void EnsureTimestamp(DateTimeOffset timestamp, string parameterName)
    {
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", parameterName);
    }
}
