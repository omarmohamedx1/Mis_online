namespace MIS.Application.DTOs.Collections;

public sealed record CollectionAttachmentDto(Guid Id, Guid CaseId, Guid? PaymentId, string Category, string OriginalFileName, string ContentType, long FileSize, string UploadedBy, DateTimeOffset UploadedAt);
public sealed record CollectionAttachmentDownloadDto(Stream Content, string ContentType, string FileName);
