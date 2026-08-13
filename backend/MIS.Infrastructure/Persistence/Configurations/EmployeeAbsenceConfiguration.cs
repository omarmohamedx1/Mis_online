using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Constants;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class EmployeeAbsenceConfiguration : IEntityTypeConfiguration<EmployeeAbsence>
{
    public void Configure(EntityTypeBuilder<EmployeeAbsence> builder)
    {
        builder.ToTable("EmployeeAbsences", table =>
        {
            table.HasCheckConstraint("CK_EmployeeAbsences_Type", $"\"Type\" = '{AbsenceValues.AbsentType}'");
            table.HasCheckConstraint("CK_EmployeeAbsences_AttendanceSource", $"\"AttendanceSource\" = '{AbsenceValues.ManualSource}'");
            table.HasCheckConstraint("CK_EmployeeAbsences_Status", $"\"Status\" IN ('{AbsenceValues.PendingStatus}', '{AbsenceValues.ExcusedStatus}', '{AbsenceValues.UnexcusedStatus}')");
            table.HasCheckConstraint("CK_EmployeeAbsences_PayrollImpactStatus", $"\"PayrollImpactStatus\" IN ('{AbsenceValues.PayrollNotApplicable}', '{AbsenceValues.PayrollPendingReview}', '{AbsenceValues.PayrollApproved}', '{AbsenceValues.PayrollExcluded}')");
            table.HasCheckConstraint("CK_EmployeeAbsences_DeductionAmounts", "\"SuggestedDeductionAmount\" >= 0 AND (\"ApprovedDeductionAmount\" IS NULL OR \"ApprovedDeductionAmount\" >= 0)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AbsenceDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.Type).HasMaxLength(24).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.Status).HasMaxLength(24).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.AttendanceSource).HasMaxLength(24).IsRequired();
        builder.Property(x => x.SuggestedDeductionAmount).HasPrecision(18, 2).HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.ApprovedDeductionAmount).HasPrecision(18, 2);
        builder.Property(x => x.PayrollImpactStatus).HasMaxLength(24).HasDefaultValue(AbsenceValues.PayrollNotApplicable).IsRequired();
        builder.Property(x => x.PayrollNotes).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PayrollReviewedByUser).WithMany().HasForeignKey(x => x.PayrollReviewedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.AbsenceDate);
        builder.HasIndex(x => new { x.EmployeeId, x.AbsenceDate }, "UX_EmployeeAbsences_Employee_Date").IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PayrollImpactStatus);
    }
}
