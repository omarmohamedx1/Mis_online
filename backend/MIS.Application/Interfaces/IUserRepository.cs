using MIS.Domain.Entities;

namespace MIS.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByLoginIdentifierAsync(string identifier, CancellationToken cancellationToken);

    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string email, Guid excludingUserId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
