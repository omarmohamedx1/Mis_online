using MIS.Application.Common;
using MIS.Application.DTOs.Auth;
using MIS.Application.Interfaces;

namespace MIS.Application.Services;

public sealed class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid username or password.";

    private readonly IPasswordHashService _passwordHashService;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHashService passwordHashService,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var usernameOrEmail = request.Username.Trim();

        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return AuthResult.Failure(InvalidCredentialsMessage);
        }

        var user = await _userRepository.FindByUsernameOrEmailAsync(usernameOrEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return AuthResult.Failure(InvalidCredentialsMessage);
        }

        var passwordStatus = _passwordHashService.VerifyPassword(user, request.Password);

        if (passwordStatus == PasswordVerificationStatus.Failed)
        {
            return AuthResult.Failure(InvalidCredentialsMessage);
        }

        var now = DateTimeOffset.UtcNow;

        if (passwordStatus == PasswordVerificationStatus.SuccessRehashNeeded)
        {
            var updatedHash = _passwordHashService.HashPassword(user, request.Password);
            user.SetPasswordHash(updatedHash, now);
        }

        user.MarkLoggedIn(now);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles
            .Select(userRole => userRole.Role.Name)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var primaryRole = roles.FirstOrDefault() ?? "User";

        var authenticatedUser = new AuthenticatedUserDto(
            user.Id,
            user.Username,
            user.FullName,
            primaryRole,
            roles);

        return AuthResult.Success(new AuthResponse(accessToken, authenticatedUser));
    }
}
