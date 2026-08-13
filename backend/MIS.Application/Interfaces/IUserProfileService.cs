using MIS.Application.DTOs.Auth;

namespace MIS.Application.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileDto> GetAsync(CancellationToken cancellationToken);
    Task<UserProfileDto> ChangeEmailAsync(ChangeMyEmailRequest request, CancellationToken cancellationToken);
    Task ChangePasswordAsync(ChangeMyPasswordRequest request, CancellationToken cancellationToken);
}
