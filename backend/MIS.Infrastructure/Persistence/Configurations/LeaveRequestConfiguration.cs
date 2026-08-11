using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Constants;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests", table =>
        {
            table.HasCheckConstraint(
                "CK_LeaveRequests_DateRange",
                "\"EndDate\" >= \"StartDate\"");
            table.HasCheckConstraint(
                "CK_LeaveRequests_NumberOfDays",
                "\"NumberOfDays\" > 0");
            table.HasCheckConstraint(
                "CK_LeaveRequests_Status",
                $"\"Status\" IN ('{LeaveRequestStatuses.Pending}', '{LeaveRequestStatuses.Approved}', '{LeaveRequestStatuses.Rejected}', '{LeaveRequestStatuses.Cancelled}')");
            table.HasCheckConstraint(
                "CK_LeaveRequests_Decision",
                $"(\"Status\" = '{LeaveRequestStatuses.Pending}' AND \"DecidedByUserId\" IS NULL AND \"DecidedAt\" IS NULL) OR " +
                $"(\"Status\" <> '{LeaveRequestStatuses.Pending}' AND \"DecidedByUserId\" IS NOT NULL AND \"DecidedAt\" IS NOT NULL)");
            table.HasCheckConstraint(
                "CK_LeaveRequests_DecisionReason",
                $"\"Status\" NOT IN ('{LeaveRequestStatuses.Rejected}', '{LeaveRequestStatuses.Cancelled}') OR " +
                "(\"DecisionNotes\" IS NOT NULL AND length(btrim(\"DecisionNotes\")) > 0)");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.StartDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.NumberOfDays).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(2000);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.RequestDate).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(24).IsRequired();
        builder.Property(x => x.DecisionNotes).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.EmployeeId, x.StartDate, x.EndDate });
        builder.HasIndex(x => new { x.EmployeeId, x.Status });
        builder.HasIndex(x => new { x.Status, x.StartDate });
        builder.HasIndex(x => x.LeaveTypeId);
        builder.HasIndex(x => x.RequestDate);
        builder.HasIndex(x => x.AttachmentDocumentId);
        builder.HasIndex(x => x.CreatedByUserId);
        builder.HasIndex(x => x.DecidedByUserId);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LeaveType)
            .WithMany()
            .HasForeignKey(x => x.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AttachmentDocument)
            .WithMany()
            .HasForeignKey(x => x.AttachmentDocumentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DecidedByUser)
            .WithMany()
            .HasForeignKey(x => x.DecidedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
