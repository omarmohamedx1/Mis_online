namespace MIS.Application.DTOs.Collections;

public sealed record BankDirectoryItemDto(
    Guid Id,
    string Code,
    string NameArabic,
    string NameEnglish,
    string? LogoUrl);
