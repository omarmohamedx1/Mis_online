using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Constants;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords", table =>
        {
            table.HasCheckConstraint("CK_AttendanceRecords_CheckTimes", "\"CheckOut\" IS NULL OR \"CheckIn\" IS NULL OR \"CheckOut\" >= \"CheckIn\"");
            table.HasCheckConstraint("CK_AttendanceRecords_Minutes", "\"WorkingMinutes\" >= 0 AND \"LateMinutes\" >= 0 AND \"EarlyLeaveMinutes\" >= 0 AND \"OvertimeMinutes\" >= 0");
            table.HasCheckConstraint("CK_AttendanceRecords_Status", $"\"Status\" IN ('{AttendanceValues.PresentStatus}', '{AttendanceValues.AbsentStatus}', '{AttendanceValues.LateStatus}', '{AttendanceValues.LeaveStatus}', '{AttendanceValues.HolidayStatus}', '{AttendanceValues.WeekendStatus}')");
            table.HasCheckConstraint("CK_AttendanceRecords_Source", $"\"Source\" IN ('{AttendanceValues.ExcelImportSource}', '{AttendanceValues.ManualSource}', '{AttendanceValues.DeviceIntegrationSource}', '{AttendanceValues.SystemProcessingSource}')");
            table.HasCheckConstraint("CK_AttendanceRecords_ImportBatch", $"\"Source\" <> '{AttendanceValues.ExcelImportSource}' OR \"ImportBatchId\" IS NOT NULL");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AttendanceDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.WorkingMinutes).IsRequired();
        builder.Ignore(x => x.WorkingHours);
        builder.Property(x => x.LateMinutes).IsRequired();
        builder.Property(x => x.EarlyLeaveMinutes).IsRequired();
        builder.Property(x => x.OvertimeMinutes).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.IsManuallyAdjusted).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.DeleteReason).HasMaxLength(500);
        builder.HasIndex(x => new { x.EmployeeId, x.AttendanceDate }, "UX_AttendanceRecords_Employee_Date")
            .IsUnique()
            .HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(x => new { x.AttendanceDate, x.Status });
        builder.HasIndex(x => new { x.ImportBatchId, x.AttendanceDate });
        builder.HasIndex(x => new { x.CreatedByUserId, x.CreatedAt });
        builder.HasIndex(x => x.UpdatedByUserId);
        builder.HasIndex(x => x.DeletedByUserId);
        builder.HasIndex(x => x.AttendanceDate, "IX_AttendanceRecords_MissingCheckOut")
            .HasFilter("\"CheckIn\" IS NOT NULL AND \"CheckOut\" IS NULL AND \"IsDeleted\" = FALSE");
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ImportBatch).WithMany().HasForeignKey(x => x.ImportBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
