using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class EmployeeLeaveEntitlementConfiguration : IEntityTypeConfiguration<EmployeeLeaveEntitlement>
{
    public void Configure(EntityTypeBuilder<EmployeeLeaveEntitlement> builder)
    {
        builder.ToTable("EmployeeLeaveEntitlements", table =>
        {
            table.HasCheckConstraint("CK_EmployeeLeaveEntitlements_Year", "\"Year\" BETWEEN 1900 AND 9999");
            table.HasCheckConstraint("CK_EmployeeLeaveEntitlements_Base", "\"BaseEntitlement\" >= 0");
            table.HasCheckConstraint("CK_EmployeeLeaveEntitlements_Total", "\"BaseEntitlement\" + \"Adjustment\" >= 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Year).IsRequired();
        builder.Property(x => x.BaseEntitlement).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Adjustment).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.EmployeeId, x.LeaveTypeId, x.Year }).IsUnique();
        builder.HasIndex(x => new { x.EmployeeId, x.Year });
        builder.HasIndex(x => new { x.LeaveTypeId, x.Year });
        builder.HasIndex(x => x.CreatedByUserId);
        builder.HasIndex(x => x.UpdatedByUserId);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LeaveType)
            .WithMany()
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
