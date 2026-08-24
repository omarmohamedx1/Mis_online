using Microsoft.EntityFrameworkCore;
using MIS.Application.Common;
using MIS.Application.DTOs.Collections;
using MIS.Application.Interfaces;
using MIS.Domain.Constants;
using MIS.Domain.Entities;
using MIS.Infrastructure.Persistence;

namespace MIS.Infrastructure.Services;

public sealed class BanksService : IBanksService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserContext _user;

    public BanksService(ApplicationDbContext db, ICurrentUserContext user)
    {
        _db = db;
        _user = user;
    }

    public async Task<IReadOnlyCollection<BankDirectoryItemDto>> GetOrganizationsAsync(string organizationType, string? search, CancellationToken token)
    {
        var query = AccessibleOrganizations(organizationType);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(bank =>
                bank.Code.ToLower().Contains(term) ||
                bank.NameArabic.ToLower().Contains(term) ||
                bank.NameEnglish.ToLower().Contains(term));
        }

        return await Project(query.OrderBy(bank => bank.NameEnglish)).ToArrayAsync(token);
    }

    public async Task<BankDirectoryItemDto> GetOrganizationAsync(string organizationType, Guid organizationId, CancellationToken token) =>
        await Project(AccessibleOrganizations(organizationType).Where(item => item.Id == organizationId)).SingleOrDefaultAsync(token)
        ?? throw new HrNotFoundException("Organization was not found.");

    private IQueryable<ClientOrganization> AccessibleOrganizations(string organizationType)
    {
        var userId = _user.UserId;
        var globalAccess = _user.Roles.Any(role => role is
            SystemRoleNames.Admin or
            SystemRoleNames.CollectionsOperationsManager or
            SystemRoleNames.CollectionsReviewer or
            SystemRoleNames.CollectionsAuditor);

        return _db.CollectionClientOrganizations.AsNoTracking().Where(bank =>
            bank.IsActive &&
            bank.OrganizationType == organizationType &&
            (globalAccess ||
             _db.CollectionUserAccess.Any(access => access.UserId == userId && access.OrganizationId == bank.Id) ||
             _db.CollectionCases.Any(collectionCase =>
                 collectionCase.Portfolio.OrganizationId == bank.Id &&
                 (collectionCase.AssignedCollectorId == userId ||
                  (collectionCase.AssignedTeamId != null && _db.CollectionTeamMembers.Any(member =>
                      member.TeamId == collectionCase.AssignedTeamId && member.UserId == userId && member.IsActive))))));
    }

    private static IQueryable<BankDirectoryItemDto> Project(IQueryable<ClientOrganization> query) =>
        query.Select(bank => new BankDirectoryItemDto(
            bank.Id,
            bank.Code,
            bank.NameArabic,
            bank.NameEnglish,
            string.IsNullOrWhiteSpace(bank.LogoStorageKey) ? null : CollectionsBrandingService.LogoUrl(bank.Id)));
}
