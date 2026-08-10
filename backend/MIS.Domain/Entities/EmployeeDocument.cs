namespace MIS.Domain.Entities;

public sealed class EmployeeDocument
{
    private EmployeeDocument() { }

    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public string DocumentType { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public User UploadedByUser { get; private set; } = null!;
    public DateTimeOffset UploadedAt { get; private set; }
}
