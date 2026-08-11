using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class WorkingDaySettingConfiguration : IEntityTypeConfiguration<WorkingDaySetting>
{
    public void Configure(EntityTypeBuilder<WorkingDaySetting> builder)
    {
        builder.ToTable("WorkingDaySettings", table =>
        {
            table.HasCheckConstraint("CK_WorkingDaySettings_DayOfWeek", "\"DayOfWeek\" BETWEEN 0 AND 6");
            table.HasCheckConstraint("CK_WorkingDaySettings_Hours", "(\"IsWorkingDay\" AND \"StartTime\" IS NOT NULL AND \"EndTime\" IS NOT NULL) OR (NOT \"IsWorkingDay\" AND \"StartTime\" IS NULL AND \"EndTime\" IS NULL)");
            table.HasCheckConstraint("CK_WorkingDaySettings_BreakMinutes", "\"BreakMinutes\" BETWEEN 0 AND 1440");
            table.HasCheckConstraint("CK_WorkingDaySettings_LateGraceMinutes", "\"LateGraceMinutes\" BETWEEN 0 AND 240");
            table.HasCheckConstraint("CK_WorkingDaySettings_EarlyLeaveGraceMinutes", "\"EarlyLeaveGraceMinutes\" BETWEEN 0 AND 240");
            table.HasCheckConstraint("CK_WorkingDaySettings_MinimumOvertimeMinutes", "\"MinimumOvertimeMinutes\" BETWEEN 0 AND 1440");
            table.HasCheckConstraint("CK_WorkingDaySettings_NonWorkingValues", "\"IsWorkingDay\" OR (\"BreakMinutes\" = 0 AND \"LateGraceMinutes\" = 0 AND \"EarlyLeaveGraceMinutes\" = 0 AND \"MinimumOvertimeMinutes\" = 0)");
        });
        builder.HasKey(x => new { x.WorkingCalendarId, x.DayOfWeek });
        builder.Property(x => x.DayOfWeek).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsWorkingDay).IsRequired();
        builder.Property(x => x.StartTime).HasColumnType("time without time zone");
        builder.Property(x => x.EndTime).HasColumnType("time without time zone");
        builder.Property(x => x.BreakMinutes).IsRequired();
        builder.Property(x => x.LateGraceMinutes).IsRequired();
        builder.Property(x => x.EarlyLeaveGraceMinutes).IsRequired();
        builder.Property(x => x.MinimumOvertimeMinutes).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
