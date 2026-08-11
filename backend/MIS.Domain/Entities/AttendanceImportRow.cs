namespace MIS.Domain.Entities;

public sealed class AttendanceImportRow
{
    private AttendanceImportRow() { }

    public AttendanceImportRow(
        Guid batchId,
        string sourceRowNumbersJson,
        string? sourceRowsJson,
        string? sourceEmployeeNumber,
        string? sourceEmployeeName,
        Guid? employeeId,
        DateOnly? attendanceDate,
        DateTimeOffset? checkIn,
        DateTimeOffset? checkOut,
        string? punchesJson,
        string? categoriesJson,
        string? errorsJson,
        bool canImport,
        DateTimeOffset createdAt)
    {
        if (batchId == Guid.Empty) throw new ArgumentException("Import batch is required.", nameof(batchId));
        if (employeeId == Guid.Empty) throw new ArgumentException("Employee identifier cannot be empty.", nameof(employeeId));
        if (checkOut < checkIn) throw new ArgumentException("Check-out cannot be before check-in.", nameof(checkOut));
        if (canImport && (!employeeId.HasValue || !attendanceDate.HasValue))
            throw new ArgumentException("Importable rows require a matched employee and attendance date.", nameof(canImport));
        if (createdAt == default) throw new ArgumentException("Timestamp is required.", nameof(createdAt));

        Id = Guid.NewGuid();
        BatchId = batchId;
        SourceRowNumbersJson = JsonText.NormalizeRequired(sourceRowNumbersJson, nameof(sourceRowNumbersJson));
        SourceRowsJson = JsonText.NormalizeRequired(sourceRowsJson, nameof(sourceRowsJson));
        SourceEmployeeNumber = NormalizeOptional(sourceEmployeeNumber);
        SourceEmployeeName = NormalizeOptional(sourceEmployeeName);
        EmployeeId = employeeId;
        AttendanceDate = attendanceDate;
        CheckIn = checkIn?.ToUniversalTime();
        CheckOut = checkOut?.ToUniversalTime();
        PunchesJson = JsonText.NormalizeRequired(punchesJson, nameof(punchesJson));
        CategoriesJson = JsonText.NormalizeRequired(categoriesJson, nameof(categoriesJson));
        ErrorsJson = JsonText.NormalizeRequired(errorsJson, nameof(errorsJson));
        CanImport = canImport;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid BatchId { get; private set; }
    public AttendanceImportBatch Batch { get; private set; } = null!;
    public string SourceRowNumbersJson { get; private set; } = "[]";
    public string SourceRowsJson { get; private set; } = "[]";
    public string? SourceEmployeeNumber { get; private set; }
    public string? SourceEmployeeName { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }
    public DateOnly? AttendanceDate { get; private set; }
    public DateTimeOffset? CheckIn { get; private set; }
    public DateTimeOffset? CheckOut { get; private set; }
    public string PunchesJson { get; private set; } = "[]";
    public string CategoriesJson { get; private set; } = "[]";
    public string ErrorsJson { get; private set; } = "[]";
    public bool CanImport { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
