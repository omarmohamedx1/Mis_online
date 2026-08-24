namespace MIS.Application.DTOs.Collections;

public sealed record BankPortfolioImportDto(
    Guid Id, Guid BankId, string BankNameArabic, string BankNameEnglish, string PortfolioName,
    string OriginalFileName, string FileType, long FileSize, int RowCount, string Status,
    Guid UploadedByUserId, string UploadedBy, DateTimeOffset UploadedAt, DateTimeOffset? ConfirmedAt,
    string? Notes, DateTimeOffset? UpdatedAt);

public sealed record BankPortfolioImportPageDto(
    IReadOnlyCollection<BankPortfolioImportDto> Items, int TotalCount, int Page, int PageSize, int TotalPages);

public sealed record UpdateBankPortfolioImportRequest(string? Notes);
public sealed record BankPortfolioReplacementPreviewDto(string Token, string OriginalFileName, string FileType, long FileSize, int RowCount);
public sealed record ConfirmBankPortfolioReplacementRequest(string Token);
