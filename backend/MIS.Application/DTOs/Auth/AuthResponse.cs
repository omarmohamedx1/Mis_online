namespace MIS.Application.DTOs.Auth;

public sealed record AuthResponse(string AccessToken, AuthenticatedUserDto User);
