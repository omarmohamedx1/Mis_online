namespace MIS.Domain.Entities;

public sealed class EmployeeContract
{
    public const string DraftStatus = "Draft";
    public const string ActiveStatus = "Active";
    public const string ExpiredStatus = "Expired";
    public const string TerminatedStatus = "Terminated";

    private EmployeeContract() { }

    public EmployeeContract(
        Guid employeeId,
        Guid contractTypeId,
        DateOnly contractStartDate,
        DateOnly? contractEndDate,
        DateOnly? probationStartDate,
        DateOnly? probationEndDate,
        string status,
        string? notes,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        EmployeeId = EnsureRequiredId(employeeId, nameof(employeeId));
        SetDetails(contractTypeId, contractStartDate, contractEndDate, probationStartDate, probationEndDate, status, notes, createdAt);
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public Guid ContractTypeId { get; private set; }
    public ContractType ContractType { get; private set; } = null!;
    public DateOnly ContractStartDate { get; private set; }
    public DateOnly? ContractEndDate { get; private set; }
    public DateOnly? ProbationStartDate { get; private set; }
    public DateOnly? ProbationEndDate { get; private set; }
    public string Status { get; private set; } = DraftStatus;
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(
        Guid contractTypeId,
        DateOnly contractStartDate,
        DateOnly? contractEndDate,
        DateOnly? probationStartDate,
        DateOnly? probationEndDate,
        string status,
        string? notes,
        DateTimeOffset updatedAt)
    {
        SetDetails(contractTypeId, contractStartDate, contractEndDate, probationStartDate, probationEndDate, status, notes, updatedAt);
        UpdatedAt = updatedAt;
    }

    public void CloseForReplacement(DateOnly replacementStartDate, DateTimeOffset updatedAt)
    {
        if (replacementStartDate == default)
            throw new ArgumentException("Replacement contract start date is required.", nameof(replacementStartDate));
        if (Status is ExpiredStatus or TerminatedStatus) return;
        if (replacementStartDate < ContractStartDate)
            throw new ArgumentException("A replacement contract cannot start before the current contract.", nameof(replacementStartDate));

        var closingDate = replacementStartDate == ContractStartDate
            ? ContractStartDate
            : replacementStartDate.AddDays(-1);
        if (ContractEndDate.HasValue && ContractEndDate.Value < closingDate)
            closingDate = ContractEndDate.Value;

        var probationStart = ProbationStartDate <= closingDate ? ProbationStartDate : null;
        var probationEnd = probationStart.HasValue && ProbationEndDate.HasValue
            ? (ProbationEndDate.Value <= closingDate ? ProbationEndDate : closingDate)
            : null;

        Update(
            ContractTypeId,
            ContractStartDate,
            closingDate,
            probationStart,
            probationEnd,
            ExpiredStatus,
            Notes,
            updatedAt);
    }

    private void SetDetails(
        Guid contractTypeId,
        DateOnly contractStartDate,
        DateOnly? contractEndDate,
        DateOnly? probationStartDate,
        DateOnly? probationEndDate,
        string status,
        string? notes,
        DateTimeOffset timestamp)
    {
        ContractTypeId = EnsureRequiredId(contractTypeId, nameof(contractTypeId));
        if (contractStartDate == default) throw new ArgumentException("Contract start date is required.", nameof(contractStartDate));
        if (contractEndDate < contractStartDate) throw new ArgumentException("Contract end date cannot be before its start date.", nameof(contractEndDate));
        if (probationEndDate.HasValue && !probationStartDate.HasValue) throw new ArgumentException("Probation start date is required when an end date is supplied.", nameof(probationStartDate));
        if (probationStartDate < contractStartDate) throw new ArgumentException("Probation cannot start before the contract.", nameof(probationStartDate));
        if (probationEndDate < probationStartDate) throw new ArgumentException("Probation end date cannot be before its start date.", nameof(probationEndDate));
        if (contractEndDate.HasValue && probationEndDate > contractEndDate) throw new ArgumentException("Probation cannot end after the contract.", nameof(probationEndDate));
        var normalizedStatus = NormalizeStatus(status)
            ?? throw new ArgumentException("Invalid contract status.", nameof(status));
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));

        ContractStartDate = contractStartDate;
        ContractEndDate = contractEndDate;
        ProbationStartDate = probationStartDate;
        ProbationEndDate = probationEndDate;
        Status = normalizedStatus;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public static bool IsValidStatus(string? status) =>
        NormalizeStatus(status) is DraftStatus or ActiveStatus or ExpiredStatus or TerminatedStatus;

    private static string? NormalizeStatus(string? status) => status?.Trim().ToLowerInvariant() switch
    {
        "draft" => DraftStatus,
        "active" => ActiveStatus,
        "expired" => ExpiredStatus,
        "terminated" => TerminatedStatus,
        _ => null
    };

    private static Guid EnsureRequiredId(Guid id, string parameterName) =>
        id == Guid.Empty ? throw new ArgumentException("Identifier is required.", parameterName) : id;
}
