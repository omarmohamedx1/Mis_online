using System.Data;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MIS.Application.Common;
using MIS.Application.DTOs.Collections;
using MIS.Application.Interfaces;
using MIS.Domain.Constants;
using MIS.Domain.Entities;
using MIS.Infrastructure.Persistence;
using Npgsql;

namespace MIS.Infrastructure.Services;

public sealed class BankPortfolioImportService : IBankPortfolioImportService
{
    public const long MaximumBytes = 20 * 1024 * 1024;
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _user;
    private readonly IHrFileStorage _files;
    private readonly ILogger<BankPortfolioImportService> _logger;
    private static readonly ConcurrentDictionary<Guid, ReplacementCandidate> Replacements = new();
    private sealed record ReplacementCandidate(Guid BankId, Guid ImportId, Guid UserId, string OriginalFileName,
        string ContentType, long FileSize, string FileHash, string StorageKey, int RowCount, DateTimeOffset ExpiresAt);

    public BankPortfolioImportService(ApplicationDbContext db, ICurrentUserContext user, IHrFileStorage files,
        ILogger<BankPortfolioImportService> logger)
    { _db = db; _user = user; _files = files; _logger = logger; }

    public async Task<BankPortfolioImportDto> UploadAsync(Guid bankId, string fileName, string contentType, long length, Stream content, CancellationToken token)
    {
        EnsureImportPermission();
        var bank = await RequireAccessibleBankAsync(bankId, token);
        if (length <= 0 || length > MaximumBytes) throw new HrValidationException("Portfolio files must be between 1 byte and 20 MB.");
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is not (".xlsx" or ".xls" or ".csv")) throw new HrValidationException("Only XLSX, XLS, and CSV portfolio files are supported.");

        var stored = await _files.SaveAsync($"bank-portfolio-imports/{bank.Id:N}", fileName, contentType, content, MaximumBytes, token);
        try
        {
            await using var storedStream = await _files.OpenReadAsync(stored.StorageKey, token);
            int rowCount;
            try { rowCount = await BankPortfolioFileInspector.CountRowsAsync(storedStream, extension, token); }
            catch (Exception exception) when (exception is not HrException and not OperationCanceledException)
            { throw new HrValidationException("The portfolio file is invalid or unreadable."); }
            var duplicate = await _db.BankPortfolioImports.AnyAsync(item => item.BankId == bankId && item.FileHash == stored.Sha256Hash && item.Status != "FAILED", token);
            if (duplicate) throw new HrConflictException("This exact file is already awaiting confirmation or has been imported for this bank.");
            var uploadedAt = DateTimeOffset.UtcNow;
            var bankName = ApiTextLocalizer.IsArabic ? bank.NameArabic : bank.NameEnglish;
            var portfolioName = $"{bankName} - {uploadedAt:dd/MM/yyyy}";
            var entity = new BankPortfolioImport(bankId, portfolioName, stored.OriginalFileName, ResolveContentType(extension), stored.Length,
                stored.Sha256Hash, stored.StorageKey, rowCount, _user.UserId, uploadedAt);
            _db.BankPortfolioImports.Add(entity);
            try { await _db.SaveChangesAsync(token); }
            catch (DbUpdateException)
            {
                _db.Entry(entity).State = EntityState.Detached;
                if (await _db.BankPortfolioImports.AsNoTracking().AnyAsync(item => item.BankId == bankId && item.FileHash == stored.Sha256Hash, token))
                    throw new HrConflictException("This exact file is already awaiting confirmation or has been imported for this bank.");
                throw;
            }
            return await GetAsync(bankId, entity.Id, token);
        }
        catch
        {
            await _files.DeleteAsync(stored.StorageKey, token);
            throw;
        }
    }

    public async Task<BankPortfolioImportDto> ConfirmAsync(Guid bankId, Guid importId, string? notes, CancellationToken token)
    {
        EnsureImportPermission(); _ = await RequireAccessibleBankAsync(bankId, token);
        ValidateNotes(notes);
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
        var entity = await _db.BankPortfolioImports.SingleOrDefaultAsync(item => item.Id == importId && item.BankId == bankId, token)
            ?? throw new HrNotFoundException("Portfolio import was not found for this bank.");
        entity.Confirm(DateTimeOffset.UtcNow);
        entity.UpdateNotes(notes, DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return await GetAsync(bankId, importId, token);
    }

    public async Task<BankPortfolioImportDto> UpdateNotesAsync(Guid bankId, Guid importId, string? notes, CancellationToken token)
    {
        EnsureImportPermission(); _ = await RequireAccessibleBankAsync(bankId, token); ValidateNotes(notes);
        var entity = await _db.BankPortfolioImports.SingleOrDefaultAsync(item => item.Id == importId && item.BankId == bankId, token)
            ?? throw new HrNotFoundException("Portfolio import was not found for this bank.");
        entity.UpdateNotes(notes, DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync(token);
        return await GetAsync(bankId, importId, token);
    }

    public async Task<BankPortfolioReplacementPreviewDto> PreviewReplacementAsync(Guid bankId, Guid importId, string fileName,
        string contentType, long length, Stream content, CancellationToken token)
    {
        EnsureImportPermission(); var bank = await RequireAccessibleBankAsync(bankId, token);
        _ = await _db.BankPortfolioImports.AsNoTracking().SingleOrDefaultAsync(item => item.Id == importId && item.BankId == bankId, token)
            ?? throw new HrNotFoundException("Portfolio import was not found for this bank.");
        if (length <= 0 || length > MaximumBytes) throw new HrValidationException("Portfolio files must be between 1 byte and 20 MB.");
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is not (".xlsx" or ".xls" or ".csv")) throw new HrValidationException("Only XLSX, XLS, and CSV portfolio files are supported.");
        var stored = await _files.SaveAsync($"bank-portfolio-imports/{bank.Id:N}/replacements", fileName, contentType, content, MaximumBytes, token);
        try
        {
            await using var storedStream = await _files.OpenReadAsync(stored.StorageKey, token);
            int rowCount;
            try { rowCount = await BankPortfolioFileInspector.CountRowsAsync(storedStream, extension, token); }
            catch (Exception exception) when (exception is not HrException and not OperationCanceledException)
            { throw new HrValidationException("The portfolio file is invalid or unreadable."); }
            if (await _db.BankPortfolioImports.AsNoTracking().AnyAsync(item => item.BankId == bankId && item.Id != importId && item.FileHash == stored.Sha256Hash, token))
                throw new HrConflictException("This exact file is already used by another import for this bank.");
            var id = Guid.NewGuid();
            Replacements[id] = new(bankId, importId, _user.UserId, stored.OriginalFileName, ResolveContentType(extension), stored.Length,
                stored.Sha256Hash, stored.StorageKey, rowCount, DateTimeOffset.UtcNow.AddMinutes(30));
            return new(id.ToString("N"), stored.OriginalFileName, extension[1..].ToUpperInvariant(), stored.Length, rowCount);
        }
        catch { await _files.DeleteAsync(stored.StorageKey, CancellationToken.None); throw; }
    }

    public async Task<BankPortfolioImportDto> ConfirmReplacementAsync(Guid bankId, Guid importId, string replacementToken, CancellationToken token)
    {
        EnsureImportPermission(); _ = await RequireAccessibleBankAsync(bankId, token);
        if (!Guid.TryParseExact(replacementToken, "N", out var candidateId) || !Replacements.TryRemove(candidateId, out var replacement) ||
            replacement.BankId != bankId || replacement.ImportId != importId || replacement.UserId != _user.UserId || replacement.ExpiresAt < DateTimeOffset.UtcNow)
            throw new HrValidationException("The replacement review has expired or is invalid. Select the file again.");
        var committed = false;
        try
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
            var entity = await _db.BankPortfolioImports.SingleOrDefaultAsync(item => item.Id == importId && item.BankId == bankId, token)
                ?? throw new HrNotFoundException("Portfolio import was not found for this bank.");
            if (await _db.BankPortfolioImports.AnyAsync(item => item.BankId == bankId && item.Id != importId && item.FileHash == replacement.FileHash, token))
                throw new HrConflictException("This exact file is already used by another import for this bank.");
            var oldStorageKey = entity.StorageKey;
            entity.ReplaceFile(replacement.OriginalFileName, replacement.ContentType, replacement.FileSize, replacement.FileHash,
                replacement.StorageKey, replacement.RowCount, DateTimeOffset.UtcNow);
            try { await _db.SaveChangesAsync(token); await transaction.CommitAsync(token); committed = true; }
            catch (DbUpdateException) { throw new HrConflictException("This exact file is already used by another import for this bank."); }
            await _files.DeleteAsync(oldStorageKey, CancellationToken.None);
            return await GetAsync(bankId, importId, token);
        }
        catch { if (!committed) await _files.DeleteAsync(replacement.StorageKey, CancellationToken.None); throw; }
    }

    public async Task DeleteAsync(Guid bankId, Guid importId, CancellationToken token)
    {
        EnsureImportPermission(); _ = await RequireAccessibleBankAsync(bankId, token);
        var storageKey = await _db.BankPortfolioImports.AsNoTracking()
            .Where(item => item.Id == importId && item.BankId == bankId)
            .Select(item => item.StorageKey)
            .SingleOrDefaultAsync(token)
            ?? throw new HrNotFoundException("Portfolio import was not found for this bank.");

        try
        {
            var deleted = await _db.BankPortfolioImports
                .Where(item => item.Id == importId && item.BankId == bankId)
                .ExecuteDeleteAsync(token);
            if (deleted == 0) throw new HrNotFoundException("Portfolio import was not found for this bank.");
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new HrConflictException("This portfolio import is in use and cannot be deleted.");
        }

        try { await _files.DeleteAsync(storageKey, CancellationToken.None); }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                "Portfolio import {ImportId} for bank {BankId} was deleted, but storage cleanup failed for key {StorageKey}.",
                importId, bankId, storageKey);
        }
    }

    public async Task<BankPortfolioImportDto> GetAsync(Guid bankId, Guid importId, CancellationToken token)
    {
        EnsureImportPermission(); _ = await RequireAccessibleBankAsync(bankId, token);
        return await Project(_db.BankPortfolioImports.AsNoTracking().Where(item => item.Id == importId && item.BankId == bankId)).SingleOrDefaultAsync(token)
            ?? throw new HrNotFoundException("Portfolio import was not found for this bank.");
    }

    public async Task<BankPortfolioImportPageDto> GetHistoryAsync(Guid bankId, int page, int pageSize, string? search, CancellationToken token)
    {
        EnsureImportPermission(); _ = await RequireAccessibleBankAsync(bankId, token);
        if (page < 1 || pageSize is < 1 or > 100) throw new HrValidationException("Page must be at least 1 and page size must be between 1 and 100.");
        var query = _db.BankPortfolioImports.AsNoTracking().Where(item => item.BankId == bankId && item.Status == "COMPLETED" && !item.IsArchived);
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim().ToLower(); query = query.Where(item => item.OriginalFileName.ToLower().Contains(term) || item.PortfolioName.ToLower().Contains(term)); }
        var total = await query.CountAsync(token);
        var items = await Project(query.OrderByDescending(item => item.UploadedAt).Skip((page - 1) * pageSize).Take(pageSize)).ToArrayAsync(token);
        return new BankPortfolioImportPageDto(items, total, page, pageSize, total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize));
    }

    private async Task<ClientOrganization> RequireAccessibleBankAsync(Guid bankId, CancellationToken token)
    {
        var global = _user.Roles.Any(role => role is SystemRoleNames.Admin or SystemRoleNames.CollectionsOperationsManager or SystemRoleNames.CollectionsReviewer or SystemRoleNames.CollectionsAuditor);
        var userId = _user.UserId;
        return await _db.CollectionClientOrganizations.AsNoTracking().SingleOrDefaultAsync(bank => bank.Id == bankId && bank.IsActive && (bank.OrganizationType == CollectionsValues.OrganizationTypes.Bank || bank.OrganizationType == CollectionsValues.OrganizationTypes.ConsumerFinance) &&
            (global || _db.CollectionUserAccess.Any(access => access.UserId == userId && access.OrganizationId == bank.Id)), token)
            ?? throw new HrNotFoundException("Bank was not found or is outside your authorized scope.");
    }

    private IQueryable<BankPortfolioImportDto> Project(IQueryable<BankPortfolioImport> query) => query.Select(item => new BankPortfolioImportDto(
        item.Id, item.BankId, item.Bank.NameArabic, item.Bank.NameEnglish, item.PortfolioName, item.OriginalFileName,
        item.ContentType == "text/csv" ? "CSV" : item.ContentType == "application/vnd.ms-excel" ? "XLS" : "XLSX",
        item.FileSize, item.RowCount, item.Status,
        item.UploadedById, item.UploadedBy.FullName, item.UploadedAt, item.ConfirmedAt, item.Notes, item.UpdatedAt));

    private void EnsureImportPermission()
    {
        if (!_user.Roles.Any(role => role.Equals(SystemRoleNames.Admin, StringComparison.OrdinalIgnoreCase) || role.Equals(SystemRoleNames.CollectionsOperationsManager, StringComparison.OrdinalIgnoreCase)))
            throw new HrForbiddenException("Only collections operations management can import bank portfolios.");
    }

    private static void ValidateNotes(string? notes)
    {
        if (notes?.Length > 1000) throw new HrValidationException("Notes cannot exceed 1000 characters.");
    }

    private static string ResolveContentType(string extension) => extension switch
    {
        ".csv" => "text/csv", ".xls" => "application/vnd.ms-excel",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _ => "application/octet-stream"
    };
}
