using MIS.Domain.Entities;

namespace MIS.Application.Interfaces;

public interface IPasswordHashService
{
    string HashPassword(User user, string password);

    PasswordVerificationStatus VerifyPassword(User user, string password);
}
