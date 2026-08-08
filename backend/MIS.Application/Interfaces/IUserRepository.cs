using MIS.Domain.Entities;

namespace MIS.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
