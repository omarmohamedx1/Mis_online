namespace MIS.Domain.Entities;

public sealed class UserRole
{
    private UserRole()
    {
    }

    public UserRole(Guid userId, Guid roleId, DateTimeOffset createdAt)
    {
        UserId = userId;
        RoleId = roleId;
        CreatedAt = createdAt;
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public Guid RoleId { get; private set; }

    public Role Role { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }
}
