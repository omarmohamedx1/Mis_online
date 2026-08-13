using System.ComponentModel.DataAnnotations;

namespace MIS.Application.DTOs.Auth;

public sealed record UserProfileDto(
    Guid Id,
    string LoginCode,
    string Username,
    string Email,
    string FullName,
    string Department,
    IReadOnlyCollection<string> Roles,
    DateTimeOffset? LastLoginAt);

public sealed record ChangeMyEmailRequest(
    [Required, EmailAddress, MaxLength(256)] string NewEmail,
    [Required, MaxLength(256)] string CurrentPassword);

public sealed record ChangeMyPasswordRequest(
    [Required, MaxLength(256)] string CurrentPassword,
    [Required, MinLength(10), MaxLength(256)] string NewPassword,
    [Required, MaxLength(256)] string ConfirmPassword);
