using MIS.Domain.Entities;
using Xunit;

namespace MIS.Domain.Tests;

public sealed class EmployeeProfileLifecycleTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 22, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Complete_profile_validates_dates_and_operational_role()
    {
        var employee = new Employee("E-200", "Ahmed Ali", Guid.NewGuid(), true, Now);
        Assert.Throws<ArgumentException>(() => employee.ApplyEmployeeProfile(Guid.NewGuid(), "COLLECTOR", new DateOnly(2026, 8, 1), null, null, null, new DateOnly(2026, 7, 31), Now));
        Assert.Throws<ArgumentException>(() => employee.ApplyEmployeeProfile(Guid.NewGuid(), "SECURITY_ADMIN", new DateOnly(2026, 8, 1), null, null, null, null, Now));
        Assert.Throws<ArgumentException>(() => employee.ApplyEmployeeProfile(Guid.NewGuid(), "ADMIN", new DateOnly(2026, 8, 1), null, new DateOnly(2027, 1, 1), null, null, Now));
    }

    [Fact]
    public void National_id_requires_exactly_fourteen_digits()
    {
        var employee = new Employee("E-202", "Sara Ali", Guid.NewGuid(), true, Now);
        Assert.Throws<ArgumentException>(() => employee.SetNationalId("2980101123456", Now));
        Assert.Throws<ArgumentException>(() => employee.SetNationalId("2980101123456A", Now));
        employee.SetNationalId("29801011234567", Now);
        Assert.Equal("29801011234567", employee.NationalId);
    }

    [Fact]
    public void Archive_and_restore_preserve_employment_information()
    {
        var employee = new Employee("E-201", "Mona Adel", Guid.NewGuid(), false, Now);
        var endDate = new DateOnly(2026, 8, 15);
        employee.ApplyEmployeeProfile(Guid.NewGuid(), "SUPERVISOR", new DateOnly(2024, 2, 1), new DateOnly(2024, 2, 2), new DateOnly(1990, 5, 3), "Cairo", endDate, Now);
        employee.Archive("Employee left company", Guid.NewGuid(), Now.AddMinutes(1));
        Assert.True(employee.IsArchived);
        Assert.Equal(endDate, employee.TerminationDate);
        employee.Restore(Now.AddMinutes(2));
        Assert.False(employee.IsArchived);
        Assert.Equal(endDate, employee.TerminationDate);
        Assert.Equal("SUPERVISOR", employee.OperationalRole);
    }
}
