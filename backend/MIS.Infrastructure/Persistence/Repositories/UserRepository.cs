using Microsoft.EntityFrameworkCore;
using MIS.Application.Interfaces;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> FindByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken)
    {
        var normalizedLookup = usernameOrEmail.Trim().ToLowerInvariant();

        return _dbContext.Users
            .Include(user => user.Department)
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(
                user => user.Username.ToLower() == normalizedLookup || user.Email.ToLower() == normalizedLookup,
                cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
