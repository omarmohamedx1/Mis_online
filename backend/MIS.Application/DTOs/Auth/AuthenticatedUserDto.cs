namespace MIS.Application.DTOs.Auth;

public sealed record AuthenticatedUserDto(
    Guid Id,
    string Username,
    string FullName,
    string Role,
    IReadOnlyCollection<string> Roles);
