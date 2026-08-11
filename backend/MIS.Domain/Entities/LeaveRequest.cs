using MIS.Domain.Constants;

namespace MIS.Domain.Entities;

public sealed class LeaveRequest
{
    private LeaveRequest() { }

    public LeaveRequest(
        Guid employeeId,
        Guid leaveTypeId,
        DateOnly startDate,
        DateOnly endDate,
        decimal numberOfDays,
        string? reason,
        string? notes,
        Guid? attachmentDocumentId,
        DateTimeOffset requestDate,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        EmployeeId = EnsureRequiredId(employeeId, nameof(employeeId));
        LeaveTypeId = EnsureRequiredId(leaveTypeId, nameof(leaveTypeId));
        CreatedByUserId = EnsureRequiredId(createdByUserId, nameof(createdByUserId));
        EnsureTimestamp(requestDate, nameof(requestDate));
        EnsureTimestamp(createdAt, nameof(createdAt));
        SetRequestDetails(startDate, endDate, numberOfDays, reason, notes, attachmentDocumentId);

        RequestDate = requestDate;
        Status = LeaveRequestStatuses.Pending;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public Guid LeaveTypeId { get; private set; }
    public LeaveType LeaveType { get; private set; } = null!;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal NumberOfDays { get; private set; }
    public string? Reason { get; private set; }
    public string? Notes { get; private set; }
    public Guid? AttachmentDocumentId { get; private set; }
    public EmployeeDocument? AttachmentDocument { get; private set; }
    public DateTimeOffset RequestDate { get; private set; }
    public string Status { get; private set; } = LeaveRequestStatuses.Pending;
    public Guid CreatedByUserId { get; private set; }
    public User CreatedByUser { get; private set; } = null!;
    public Guid? DecidedByUserId { get; private set; }
    public User? DecidedByUser { get; private set; }
    public DateTimeOffset? DecidedAt { get; private set; }
    public string? DecisionNotes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(
        Guid employeeId,
        Guid leaveTypeId,
        DateOnly startDate,
        DateOnly endDate,
        decimal numberOfDays,
        string? reason,
        string? notes,
        Guid? attachmentDocumentId,
        DateTimeOffset updatedAt)
    {
        EnsurePending();
        EnsureTimestamp(updatedAt, nameof(updatedAt));
        if (updatedAt < CreatedAt)
            throw new ArgumentException("Update timestamp cannot be before creation.", nameof(updatedAt));

        var validatedEmployeeId = EnsureRequiredId(employeeId, nameof(employeeId));
        var validatedLeaveTypeId = EnsureRequiredId(leaveTypeId, nameof(leaveTypeId));
        SetRequestDetails(startDate, endDate, numberOfDays, reason, notes, attachmentDocumentId);
        EmployeeId = validatedEmployeeId;
        LeaveTypeId = validatedLeaveTypeId;
        UpdatedAt = updatedAt;
    }

    public void Approve(Guid decidedByUserId, string? notes, DateTimeOffset decidedAt) =>
        TransitionTo(LeaveRequestStatuses.Approved, decidedByUserId, notes, false, false, decidedAt);

    public void Reject(Guid decidedByUserId, string reason, DateTimeOffset decidedAt) =>
        TransitionTo(LeaveRequestStatuses.Rejected, decidedByUserId, reason, true, false, decidedAt);

    public void Cancel(Guid decidedByUserId, string reason, DateTimeOffset decidedAt) =>
        TransitionTo(LeaveRequestStatuses.Cancelled, decidedByUserId, reason, true, true, decidedAt);

    private void SetRequestDetails(
        DateOnly startDate,
        DateOnly endDate,
        decimal numberOfDays,
        string? reason,
        string? notes,
        Guid? attachmentDocumentId)
    {
        if (startDate == default) throw new ArgumentException("Start date is required.", nameof(startDate));
        if (endDate == default) throw new ArgumentException("End date is required.", nameof(endDate));
        if (endDate < startDate) throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        if (numberOfDays <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfDays), "Number of days must be greater than zero.");
        EnsureOptionalId(attachmentDocumentId, nameof(attachmentDocumentId));

        StartDate = startDate;
        EndDate = endDate;
        NumberOfDays = numberOfDays;
        Reason = NormalizeOptional(reason);
        Notes = NormalizeOptional(notes);
        AttachmentDocumentId = attachmentDocumentId;
    }

    private void TransitionTo(
        string targetStatus,
        Guid decidedByUserId,
        string? decisionNotes,
        bool decisionNotesRequired,
        bool allowApprovedSource,
        DateTimeOffset decidedAt)
    {
        if (Status != LeaveRequestStatuses.Pending && !(allowApprovedSource && Status == LeaveRequestStatuses.Approved))
            throw new InvalidOperationException($"A {Status} leave request cannot be changed.");
        var validatedDecidedByUserId = EnsureRequiredId(decidedByUserId, nameof(decidedByUserId));
        EnsureTimestamp(decidedAt, nameof(decidedAt));
        if (decidedAt < RequestDate)
            throw new ArgumentException("Decision timestamp cannot be before the request date.", nameof(decidedAt));
        if (!LeaveRequestStatuses.IsValid(targetStatus) || targetStatus == LeaveRequestStatuses.Pending)
            throw new ArgumentException("Invalid leave request transition.", nameof(targetStatus));
        if (decisionNotesRequired && string.IsNullOrWhiteSpace(decisionNotes))
            throw new ArgumentException("Decision reason is required.", nameof(decisionNotes));

        Status = targetStatus;
        DecidedByUserId = validatedDecidedByUserId;
        DecisionNotes = NormalizeOptional(decisionNotes);
        DecidedAt = decidedAt;
        UpdatedAt = decidedAt;
    }

    private void EnsurePending()
    {
        if (Status != LeaveRequestStatuses.Pending)
            throw new InvalidOperationException($"A {Status} leave request cannot be changed.");
    }

    private static Guid EnsureRequiredId(Guid id, string parameterName) =>
        id == Guid.Empty ? throw new ArgumentException("Identifier is required.", parameterName) : id;

    private static void EnsureOptionalId(Guid? id, string parameterName)
    {
        if (id == Guid.Empty) throw new ArgumentException("Identifier cannot be empty.", parameterName);
    }

    private static void EnsureTimestamp(DateTimeOffset timestamp, string parameterName)
    {
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", parameterName);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
