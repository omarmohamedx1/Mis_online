using MIS.Application.Common;
using MIS.Application.DTOs.Auth;

namespace MIS.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
