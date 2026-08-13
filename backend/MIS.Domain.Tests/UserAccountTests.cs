using MIS.Domain.Entities;
using Xunit;

namespace MIS.Domain.Tests;

public sealed class UserAccountTests
{
    [Fact]
    public void NewUserGetsStableFormattedLoginCodeAndNormalizedEmail()
    {
        var user = new User("operator", " Operator@MIS.Local ", "hash", "Operator", Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Matches("^USR-[A-F0-9]{8}$", user.LoginCode);
        Assert.Equal("operator@mis.local", user.Email);
    }

    [Fact]
    public void EmailCanChangeWithoutChangingPermanentLoginCode()
    {
        var user = new User("operator", "first@mis.local", "hash", "Operator", Guid.NewGuid(), DateTimeOffset.UtcNow);
        var code = user.LoginCode;

        user.UpdateEmail(" Second@MIS.Local ", DateTimeOffset.UtcNow);

        Assert.Equal("second@mis.local", user.Email);
        Assert.Equal(code, user.LoginCode);
    }
}
