using MIS.Domain.Constants;

namespace MIS.Domain.Entities;

public sealed class EmployeeDelegation
{
    private EmployeeDelegation() { }

    public EmployeeDelegation(
        string delegationNumber,
        Guid employeeId,
        Guid delegationTypeId,
        string subject,
        string? authorizedEntity,
        DateOnly startDate,
        DateOnly endDate,
        string purpose,
        string? notes,
        string status,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (delegationTypeId == Guid.Empty) throw new ArgumentException("Delegation type is required.", nameof(delegationTypeId));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("Creator is required.", nameof(createdByUserId));
        Id = Guid.NewGuid();
        DelegationNumber = Required(delegationNumber, nameof(delegationNumber), 50).ToUpperInvariant();
        EmployeeId = employeeId;
        DelegationTypeId = delegationTypeId;
        SetDetails(subject, authorizedEntity, startDate, endDate, purpose, notes, status);
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string DelegationNumber { get; private set; } = string.Empty;
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public Guid DelegationTypeId { get; private set; }
    public DelegationType DelegationType { get; private set; } = null!;
    public string Subject { get; private set; } = string.Empty;
    public string? AuthorizedEntity { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string Purpose { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public string Status { get; private set; } = DelegationStatuses.Draft;
    public Guid CreatedByUserId { get; private set; }
    public User CreatedByUser { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }
    public User? UpdatedByUser { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public User? CancelledByUser { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public void Update(
        Guid employeeId,
        Guid delegationTypeId,
        string subject,
        string? authorizedEntity,
        DateOnly startDate,
        DateOnly endDate,
        string purpose,
        string? notes,
        string status,
        Guid updatedByUserId,
        DateTimeOffset updatedAt)
    {
        if (Status == DelegationStatuses.Cancelled) throw new InvalidOperationException("A cancelled delegation cannot be edited.");
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee is required.", nameof(employeeId));
        if (delegationTypeId == Guid.Empty) throw new ArgumentException("Delegation type is required.", nameof(delegationTypeId));
        EmployeeId = employeeId;
        DelegationTypeId = delegationTypeId;
        SetDetails(subject, authorizedEntity, startDate, endDate, purpose, notes, status);
        UpdatedByUserId = RequiredUser(updatedByUserId, nameof(updatedByUserId));
        UpdatedAt = updatedAt;
    }

    public void Cancel(string reason, Guid cancelledByUserId, DateTimeOffset cancelledAt)
    {
        if (Status == DelegationStatuses.Cancelled) throw new InvalidOperationException("The delegation is already cancelled.");
        var validatedReason = Required(reason, nameof(reason), 500);
        var validatedUserId = RequiredUser(cancelledByUserId, nameof(cancelledByUserId));
        Status = DelegationStatuses.Cancelled;
        CancellationReason = validatedReason;
        CancelledByUserId = validatedUserId;
        CancelledAt = cancelledAt;
        UpdatedByUserId = cancelledByUserId;
        UpdatedAt = cancelledAt;
    }

    private void SetDetails(
        string subject,
        string? authorizedEntity,
        DateOnly startDate,
        DateOnly endDate,
        string purpose,
        string? notes,
        string status)
    {
        if (endDate < startDate) throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        var normalizedStatus = DelegationStatuses.Normalize(status);
        if (normalizedStatus == DelegationStatuses.Cancelled)
            throw new ArgumentException("Use the cancel operation to cancel a delegation.", nameof(status));
        Subject = Required(subject, nameof(subject), 250);
        AuthorizedEntity = Optional(authorizedEntity, 250);
        StartDate = startDate;
        EndDate = endDate;
        Purpose = Required(purpose, nameof(purpose), 2000);
        Notes = Optional(notes, 2000);
        Status = normalizedStatus;
    }

    private static Guid RequiredUser(Guid value, string parameterName) =>
        value == Guid.Empty ? throw new ArgumentException("User is required.", parameterName) : value;

    private static string Required(string value, string parameterName, int maximumLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maximumLength) throw new ArgumentException($"Value cannot exceed {maximumLength} characters.", parameterName);
        return normalized;
    }

    private static string? Optional(string? value, int maximumLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized?.Length > maximumLength) throw new ArgumentException($"Value cannot exceed {maximumLength} characters.");
        return normalized;
    }
}
