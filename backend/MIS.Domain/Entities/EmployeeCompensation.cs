namespace MIS.Domain.Entities;

public sealed class EmployeeCompensation
{
    private EmployeeCompensation() { }

    public EmployeeCompensation(
        Guid employeeId,
        decimal basicSalary,
        decimal allowances,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool isCurrent,
        string? bankName,
        string? bankAccountNumber,
        string? iban,
        string? notes,
        DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        EmployeeId = EnsureRequiredId(employeeId, nameof(employeeId));
        SetDetails(basicSalary, allowances, effectiveFrom, effectiveTo, isCurrent, bankName, bankAccountNumber, iban, notes, createdAt);
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public decimal BasicSalary { get; private set; }
    public decimal Allowances { get; private set; }
    public decimal TotalSalary => BasicSalary + Allowances;
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public bool IsCurrent { get; private set; }
    public string? BankName { get; private set; }
    public string? BankAccountNumber { get; private set; }
    public string? Iban { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(
        decimal basicSalary,
        decimal allowances,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool isCurrent,
        string? bankName,
        string? bankAccountNumber,
        string? iban,
        string? notes,
        DateTimeOffset updatedAt)
    {
        SetDetails(basicSalary, allowances, effectiveFrom, effectiveTo, isCurrent, bankName, bankAccountNumber, iban, notes, updatedAt);
        UpdatedAt = updatedAt;
    }

    public void Close(DateOnly effectiveTo, DateTimeOffset updatedAt)
    {
        if (!IsCurrent) throw new InvalidOperationException("Only the current compensation record can be closed.");

        Update(
            BasicSalary,
            Allowances,
            EffectiveFrom,
            effectiveTo,
            false,
            BankName,
            BankAccountNumber,
            Iban,
            Notes,
            updatedAt);
    }

    private void SetDetails(
        decimal basicSalary,
        decimal allowances,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        bool isCurrent,
        string? bankName,
        string? bankAccountNumber,
        string? iban,
        string? notes,
        DateTimeOffset timestamp)
    {
        if (basicSalary < 0) throw new ArgumentOutOfRangeException(nameof(basicSalary));
        if (allowances < 0) throw new ArgumentOutOfRangeException(nameof(allowances));
        if (effectiveFrom == default) throw new ArgumentException("Effective start date is required.", nameof(effectiveFrom));
        if (effectiveTo < effectiveFrom) throw new ArgumentException("Effective end date cannot be before its start date.", nameof(effectiveTo));
        if (isCurrent && effectiveTo.HasValue) throw new ArgumentException("A current compensation record cannot have an end date.", nameof(effectiveTo));
        if (timestamp == default) throw new ArgumentException("Timestamp is required.", nameof(timestamp));

        BasicSalary = basicSalary;
        Allowances = allowances;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        IsCurrent = isCurrent;
        BankName = NormalizeOptional(bankName);
        BankAccountNumber = NormalizeOptional(bankAccountNumber);
        Iban = NormalizeOptional(iban)?.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        Notes = NormalizeOptional(notes);
    }

    private static Guid EnsureRequiredId(Guid id, string parameterName) =>
        id == Guid.Empty ? throw new ArgumentException("Identifier is required.", parameterName) : id;

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
