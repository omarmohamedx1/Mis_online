using MIS.Domain.Constants;
using MIS.Domain.Entities;
using Xunit;

namespace MIS.Domain.Tests;

public sealed class CoreHrDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 11, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Employee_termination_requires_a_date_and_preserves_identity()
    {
        var employee = new Employee("EMP-100", "Ahmed Ali", Guid.NewGuid(), true, Now);

        Assert.Throws<ArgumentException>(() =>
            employee.ChangeStatus(Employee.TerminatedStatus, false, null, "Resigned", Now.AddMinutes(1)));

        employee.Terminate(new DateOnly(2026, 8, 11), "Resigned", Now.AddMinutes(2));
        Assert.Equal(Employee.TerminatedStatus, employee.Status);
        Assert.False(employee.IsActive);
        Assert.NotEqual(Guid.Empty, employee.Id);
    }

    [Fact]
    public void Contract_rejects_invalid_contract_and_probation_ranges()
    {
        Assert.Throws<ArgumentException>(() => new EmployeeContract(
            Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 8, 1), new DateOnly(2026, 7, 31),
            null, null, EmployeeContract.ActiveStatus, null, Now));

        Assert.Throws<ArgumentException>(() => new EmployeeContract(
            Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 8, 1), new DateOnly(2027, 7, 31),
            new DateOnly(2026, 7, 1), new DateOnly(2026, 10, 1), EmployeeContract.ActiveStatus, null, Now));
    }

    [Fact]
    public void Contract_replacement_closes_the_previous_version_without_erasing_it()
    {
        var contract = new EmployeeContract(
            Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1), null,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31), EmployeeContract.ActiveStatus, "Original", Now);

        contract.CloseForReplacement(new DateOnly(2026, 7, 1), Now.AddMinutes(1));

        Assert.Equal(EmployeeContract.ExpiredStatus, contract.Status);
        Assert.Equal(new DateOnly(2026, 6, 30), contract.ContractEndDate);
        Assert.Equal("Original", contract.Notes);
        Assert.NotNull(contract.UpdatedAt);
    }

    [Fact]
    public void Contract_replacement_with_the_same_start_date_has_an_explicit_same_day_close()
    {
        var startDate = new DateOnly(2026, 1, 1);
        var contract = new EmployeeContract(
            Guid.NewGuid(), Guid.NewGuid(), startDate, null,
            null, null, EmployeeContract.DraftStatus, null, Now);

        contract.CloseForReplacement(startDate, Now.AddMinutes(1));

        Assert.Equal(EmployeeContract.ExpiredStatus, contract.Status);
        Assert.Equal(startDate, contract.ContractEndDate);
    }

    [Fact]
    public void Contract_replacement_does_not_rewrite_a_terminated_contract()
    {
        var originalEnd = new DateOnly(2026, 6, 30);
        var contract = new EmployeeContract(
            Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1), originalEnd,
            null, null, EmployeeContract.TerminatedStatus, "Termination history", Now);

        contract.CloseForReplacement(new DateOnly(2026, 7, 1), Now.AddMinutes(1));

        Assert.Equal(EmployeeContract.TerminatedStatus, contract.Status);
        Assert.Equal(originalEnd, contract.ContractEndDate);
        Assert.Null(contract.UpdatedAt);
    }

    [Fact]
    public void Compensation_close_preserves_financial_values_and_marks_the_version_historical()
    {
        var compensation = new EmployeeCompensation(
            Guid.NewGuid(), 10_000m, 2_000m, new DateOnly(2026, 1, 1), null, true,
            "Bank", "123456789", "EG123456789", "Original", Now);

        compensation.Close(new DateOnly(2026, 8, 10), Now.AddMinutes(1));

        Assert.False(compensation.IsCurrent);
        Assert.Equal(new DateOnly(2026, 8, 10), compensation.EffectiveTo);
        Assert.Equal(12_000m, compensation.TotalSalary);
        Assert.Equal("EG123456789", compensation.Iban);
        Assert.Throws<InvalidOperationException>(() =>
            compensation.Close(new DateOnly(2026, 8, 11), Now.AddMinutes(2)));
    }

    [Fact]
    public void Leave_request_can_cancel_an_approval_but_rejects_further_changes()
    {
        var request = new LeaveRequest(
            Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 8, 12), new DateOnly(2026, 8, 13),
            2m, "Annual leave", null, null, Now, Guid.NewGuid(), Now);

        request.Approve(Guid.NewGuid(), "Approved", Now.AddMinutes(5));

        Assert.Equal(LeaveRequestStatuses.Approved, request.Status);
        request.Cancel(Guid.NewGuid(), "Changed plans", Now.AddMinutes(10));
        Assert.Equal(LeaveRequestStatuses.Cancelled, request.Status);
        Assert.Throws<InvalidOperationException>(() => request.Update(
            request.EmployeeId, request.LeaveTypeId, request.StartDate, request.EndDate, 2m,
            request.Reason, request.Notes, null, Now.AddMinutes(10)));
    }

    [Fact]
    public void Delegation_validates_dates_and_cancellation_reason()
    {
        Assert.Throws<ArgumentException>(() => new EmployeeDelegation(
            "DEL-1", Guid.NewGuid(), Guid.NewGuid(), "Collect documents", null,
            new DateOnly(2026, 8, 12), new DateOnly(2026, 8, 11), "Collection", null,
            DelegationStatuses.Active, Guid.NewGuid(), Now));

        var delegation = new EmployeeDelegation(
            "DEL-2", Guid.NewGuid(), Guid.NewGuid(), "Collect documents", "Bank",
            new DateOnly(2026, 8, 11), new DateOnly(2026, 8, 20), "Collection", null,
            DelegationStatuses.Active, Guid.NewGuid(), Now);
        Assert.Throws<ArgumentException>(() => delegation.Cancel(" ", Guid.NewGuid(), Now.AddMinutes(1)));
        delegation.Cancel("No longer required", Guid.NewGuid(), Now.AddMinutes(2));
        Assert.Equal(DelegationStatuses.Cancelled, delegation.Status);
    }

    [Fact]
    public void Attendance_rejects_reverse_times_and_cannot_change_after_soft_delete()
    {
        var employeeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var checkIn = Now;
        Assert.Throws<ArgumentException>(() => new AttendanceRecord(
            employeeId, new DateOnly(2026, 8, 11), checkIn, checkIn.AddHours(-1), 0, 0, 0, 0,
            AttendanceValues.PresentStatus, AttendanceValues.ManualSource, null, null, true, userId, Now));

        var record = new AttendanceRecord(
            employeeId, new DateOnly(2026, 8, 11), checkIn, checkIn.AddHours(8), 420, 0, 0, 0,
            AttendanceValues.PresentStatus, AttendanceValues.ManualSource, null, null, true, userId, Now);
        record.Delete(userId, "Correction", Now.AddMinutes(1));
        Assert.True(record.IsDeleted);
        Assert.Throws<InvalidOperationException>(() => record.UpdateSummary(
            employeeId, record.AttendanceDate, checkIn, checkIn.AddHours(7), 360, 0, 0, 0,
            AttendanceValues.PresentStatus, null, true, userId, Now.AddMinutes(2)));
    }

    [Fact]
    public void Attendance_persistence_values_are_normalized_to_utc_without_changing_the_instant()
    {
        var employeeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var localCheckIn = new DateTimeOffset(2026, 8, 11, 9, 15, 0, TimeSpan.FromHours(3));
        var localCheckOut = localCheckIn.AddHours(8);
        var record = new AttendanceRecord(
            employeeId, new DateOnly(2026, 8, 11), localCheckIn, localCheckOut, 420, 15, 0, 0,
            AttendanceValues.LateStatus, AttendanceValues.ManualSource, null, null, true, userId, Now);
        var punch = new AttendancePunch(
            record.Id, localCheckIn, AttendanceValues.CheckInPunch, AttendanceValues.ManualSource,
            null, null, null, Now);
        var importRow = new AttendanceImportRow(
            Guid.NewGuid(), "[1]", "[]", "EMP-100", "Ahmed Ali", employeeId,
            new DateOnly(2026, 8, 11), localCheckIn, localCheckOut, "[]", "[\"Valid\"]", "[]", true, Now);

        Assert.Equal(TimeSpan.Zero, record.CheckIn!.Value.Offset);
        Assert.Equal(TimeSpan.Zero, record.CheckOut!.Value.Offset);
        Assert.Equal(TimeSpan.Zero, punch.Timestamp.Offset);
        Assert.Equal(TimeSpan.Zero, importRow.CheckIn!.Value.Offset);
        Assert.Equal(TimeSpan.Zero, importRow.CheckOut!.Value.Offset);
        Assert.Equal(localCheckIn.UtcDateTime, record.CheckIn.Value.UtcDateTime);
        Assert.Equal(localCheckOut.UtcDateTime, record.CheckOut.Value.UtcDateTime);
        Assert.Equal(localCheckIn.UtcDateTime, punch.Timestamp.UtcDateTime);
    }

    [Fact]
    public void Employee_document_checks_date_range_and_file_hash()
    {
        const string hash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        Assert.Throws<ArgumentException>(() => new EmployeeDocument(
            Guid.NewGuid(), Guid.NewGuid(), "Contract", "contract.pdf", "employee-documents/a.pdf",
            "application/pdf", 100, hash, new DateOnly(2026, 8, 2), new DateOnly(2026, 8, 1),
            null, Guid.NewGuid(), Now));

        Assert.Throws<ArgumentException>(() => new EmployeeDocument(
            Guid.NewGuid(), Guid.NewGuid(), "Contract", "contract.pdf", "employee-documents/a.pdf",
            "application/pdf", 100, "not-a-sha256", null, null, null, Guid.NewGuid(), Now));
    }

    [Fact]
    public void Working_calendar_keeps_weekend_rules_in_configured_day_settings()
    {
        var calendar = new WorkingCalendar("Egypt calendar", "Africa/Cairo", Now);
        foreach (var day in Enum.GetValues<DayOfWeek>())
        {
            var working = day is not DayOfWeek.Friday and not DayOfWeek.Saturday;
            calendar.SetDay(day, working, working ? new TimeOnly(9, 0) : null, working ? new TimeOnly(17, 0) : null,
                working ? 60 : 0, working ? 15 : 0, working ? 15 : 0, working ? 30 : 0, Now);
        }

        Assert.Equal(7, calendar.Days.Count);
        Assert.False(calendar.Days.Single(day => day.DayOfWeek == DayOfWeek.Friday).IsWorkingDay);
        Assert.True(calendar.Days.Single(day => day.DayOfWeek == DayOfWeek.Sunday).IsWorkingDay);
    }
}
