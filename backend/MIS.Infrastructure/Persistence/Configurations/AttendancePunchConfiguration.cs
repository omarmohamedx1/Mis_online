using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Constants;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class AttendancePunchConfiguration : IEntityTypeConfiguration<AttendancePunch>
{
    public void Configure(EntityTypeBuilder<AttendancePunch> builder)
    {
        builder.ToTable("AttendancePunches", table =>
        {
            table.HasCheckConstraint("CK_AttendancePunches_PunchType", $"\"PunchType\" IN ('{AttendanceValues.CheckInPunch}', '{AttendanceValues.CheckOutPunch}', '{AttendanceValues.UnknownPunch}')");
            table.HasCheckConstraint("CK_AttendancePunches_Source", $"\"Source\" IN ('{AttendanceValues.ExcelImportSource}', '{AttendanceValues.ManualSource}', '{AttendanceValues.DeviceIntegrationSource}')");
            table.HasCheckConstraint("CK_AttendancePunches_SourceRow", "\"SourceRowNumber\" IS NULL OR \"SourceRowNumber\" > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Timestamp).IsRequired();
        builder.Property(x => x.PunchType).HasMaxLength(24).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(32).IsRequired();
        builder.Property(x => x.RawValue).HasMaxLength(500);
        builder.Property(x => x.RawDataJson).HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => new { x.AttendanceRecordId, x.Timestamp, x.PunchType }).IsUnique();
        builder.HasIndex(x => x.Timestamp);
        builder.HasOne(x => x.AttendanceRecord)
            .WithMany()
            .HasForeignKey(x => x.AttendanceRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
