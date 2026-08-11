using MIS.Domain.Constants;

namespace MIS.Domain.Entities;

public sealed class AttendanceImportBatch
{
    private AttendanceImportBatch() { }

    public AttendanceImportBatch(
        string fileName,
        string? contentType,
        long fileSize,
        string fileHash,
        string storageKey,
        Guid uploadedByUserId,
        DateTimeOffset uploadedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        if (fileSize <= 0) throw new ArgumentOutOfRangeException(nameof(fileSize));
        if (uploadedByUserId == Guid.Empty) throw new ArgumentException("Uploading user is required.", nameof(uploadedByUserId));
        if (uploadedAt == default) throw new ArgumentException("Upload timestamp is required.", nameof(uploadedAt));

        Id = Guid.NewGuid();
        FileName = fileName.Trim();
        ContentType = NormalizeOptional(contentType);
        FileSize = fileSize;
        FileHash = fileHash.Trim().ToLowerInvariant();
        StorageKey = storageKey.Trim();
        Status = AttendanceValues.UploadedBatchStatus;
        UploadedByUserId = uploadedByUserId;
        UploadedAt = uploadedAt;
    }

    public Guid Id { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string? ContentType { get; private set; }
    public long FileSize { get; private set; }
    public string FileHash { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public string Status { get; private set; } = AttendanceValues.UploadedBatchStatus;
    public string? MappingJson { get; private set; }
    public int TotalRows { get; private set; }
    public int ValidRows { get; private set; }
    public int InvalidRows { get; private set; }
    public int EmployeeNotFoundRows { get; private set; }
    public int DuplicateRows { get; private set; }
    public int MissingCheckInRows { get; private set; }
    public int MissingCheckOutRows { get; private set; }
    public string? FailureReason { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public User UploadedByUser { get; private set; } = null!;
    public DateTimeOffset UploadedAt { get; private set; }
    public DateTimeOffset? PreviewedAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? FailedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public int ImportedRecords { get; private set; }
    public string? Notes { get; private set; }

    public void SetPreview(
        string mappingJson,
        int totalRows,
        int validRows,
        int invalidRows,
        int employeeNotFoundRows,
        int duplicateRows,
        int missingCheckInRows,
        int missingCheckOutRows,
        DateTimeOffset previewedAt)
    {
        EnsureStatus(AttendanceValues.UploadedBatchStatus, AttendanceValues.PreviewReadyBatchStatus);
        EnsureCounts(totalRows, validRows, invalidRows, employeeNotFoundRows, duplicateRows, missingCheckInRows, missingCheckOutRows);
        if (previewedAt == default) throw new ArgumentException("Preview timestamp is required.", nameof(previewedAt));

        MappingJson = JsonText.NormalizeRequired(mappingJson, nameof(mappingJson), "{}");
        TotalRows = totalRows;
        ValidRows = validRows;
        InvalidRows = invalidRows;
        EmployeeNotFoundRows = employeeNotFoundRows;
        DuplicateRows = duplicateRows;
        MissingCheckInRows = missingCheckInRows;
        MissingCheckOutRows = missingCheckOutRows;
        Status = AttendanceValues.PreviewReadyBatchStatus;
        FailureReason = null;
        PreviewedAt = previewedAt;
        UpdatedAt = previewedAt;
    }

    public void Confirm(int importedRecords, string? notes, DateTimeOffset confirmedAt)
    {
        EnsureStatus(AttendanceValues.PreviewReadyBatchStatus);
        if (importedRecords < 0 || importedRecords > ValidRows) throw new ArgumentOutOfRangeException(nameof(importedRecords));
        if (confirmedAt == default) throw new ArgumentException("Confirmation timestamp is required.", nameof(confirmedAt));

        Status = AttendanceValues.ConfirmedBatchStatus;
        ImportedRecords = importedRecords;
        Notes = NormalizeOptional(notes);
        ConfirmedAt = confirmedAt;
        UpdatedAt = confirmedAt;
    }

    public void Fail(string reason, DateTimeOffset failedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (Status == AttendanceValues.ConfirmedBatchStatus) throw new InvalidOperationException("A confirmed import cannot be failed.");
        if (failedAt == default) throw new ArgumentException("Failure timestamp is required.", nameof(failedAt));

        Status = AttendanceValues.FailedBatchStatus;
        FailureReason = reason.Trim();
        FailedAt = failedAt;
        UpdatedAt = failedAt;
    }

    public void Cancel(string? notes, DateTimeOffset cancelledAt)
    {
        if (Status == AttendanceValues.ConfirmedBatchStatus) throw new InvalidOperationException("A confirmed import cannot be cancelled.");
        if (cancelledAt == default) throw new ArgumentException("Cancellation timestamp is required.", nameof(cancelledAt));

        Status = AttendanceValues.CancelledBatchStatus;
        Notes = NormalizeOptional(notes);
        CancelledAt = cancelledAt;
        UpdatedAt = cancelledAt;
    }

    private void EnsureStatus(params string[] allowedStatuses)
    {
        if (!allowedStatuses.Contains(Status, StringComparer.Ordinal))
            throw new InvalidOperationException($"Import batch in '{Status}' status cannot perform this operation.");
    }

    private static void EnsureCounts(params int[] counts)
    {
        if (counts.Any(x => x < 0)) throw new ArgumentOutOfRangeException(nameof(counts), "Import counts cannot be negative.");
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
