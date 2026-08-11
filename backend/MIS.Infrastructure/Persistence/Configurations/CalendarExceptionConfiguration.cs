using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Constants;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class CalendarExceptionConfiguration : IEntityTypeConfiguration<CalendarException>
{
    public void Configure(EntityTypeBuilder<CalendarException> builder)
    {
        builder.ToTable("CalendarExceptions", table =>
        {
            table.HasCheckConstraint("CK_CalendarExceptions_Type", $"\"Type\" IN ('{CalendarValues.OfficialHolidayType}', '{CalendarValues.CompanyHolidayType}', '{CalendarValues.SpecialDayType}')");
            table.HasCheckConstraint("CK_CalendarExceptions_OverrideMode", $"\"OverrideMode\" IN ('{CalendarValues.NonWorkingDayOverride}', '{CalendarValues.WorkingDayOverride}', '{CalendarValues.CustomWorkingHoursOverride}')");
            table.HasCheckConstraint("CK_CalendarExceptions_CustomHours", $"\"OverrideMode\" <> '{CalendarValues.CustomWorkingHoursOverride}' OR (\"StartTime\" IS NOT NULL AND \"EndTime\" IS NOT NULL)");
            table.HasCheckConstraint("CK_CalendarExceptions_BreakMinutes", "\"BreakMinutes\" IS NULL OR \"BreakMinutes\" BETWEEN 0 AND 1440");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameEnglish).HasMaxLength(120).IsRequired();
        builder.Property(x => x.NameArabic).HasMaxLength(120);
        builder.Property(x => x.Date).HasColumnType("date").IsRequired();
        builder.Property(x => x.Type).HasMaxLength(32).IsRequired();
        builder.Property(x => x.OverrideMode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.StartTime).HasColumnType("time without time zone");
        builder.Property(x => x.EndTime).HasColumnType("time without time zone");
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.DeleteReason).HasMaxLength(500);
        builder.HasIndex(x => new { x.WorkingCalendarId, x.Date }, "UX_CalendarExceptions_Calendar_Date")
            .IsUnique()
            .HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(x => new { x.Date, x.IsActive });
        builder.HasIndex(x => new { x.Type, x.Date });
        builder.HasIndex(x => x.CreatedByUserId);
        builder.HasIndex(x => x.UpdatedByUserId);
        builder.HasIndex(x => x.DeletedByUserId);
        builder.HasOne(x => x.WorkingCalendar).WithMany().HasForeignKey(x => x.WorkingCalendarId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
