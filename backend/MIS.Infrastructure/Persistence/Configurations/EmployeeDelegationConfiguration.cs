using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Constants;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class EmployeeDelegationConfiguration : IEntityTypeConfiguration<EmployeeDelegation>
{
    public void Configure(EntityTypeBuilder<EmployeeDelegation> builder)
    {
        builder.ToTable("EmployeeDelegations", table =>
        {
            table.HasCheckConstraint("CK_EmployeeDelegations_DateRange", "\"EndDate\" >= \"StartDate\"");
            table.HasCheckConstraint("CK_EmployeeDelegations_Status", $"\"Status\" IN ('{DelegationStatuses.Draft}', '{DelegationStatuses.Active}', '{DelegationStatuses.Expired}', '{DelegationStatuses.Cancelled}')");
            table.HasCheckConstraint("CK_EmployeeDelegations_CancelState", $"(\"Status\" <> '{DelegationStatuses.Cancelled}') OR (\"CancelledAt\" IS NOT NULL AND \"CancelledByUserId\" IS NOT NULL AND \"CancellationReason\" IS NOT NULL)");
        });
        builder.HasKey(item => item.Id);
        builder.Property(item => item.DelegationNumber).HasMaxLength(50).IsRequired();
        builder.Property(item => item.Subject).HasMaxLength(250).IsRequired();
        builder.Property(item => item.AuthorizedEntity).HasMaxLength(250);
        builder.Property(item => item.StartDate).HasColumnType("date").IsRequired();
        builder.Property(item => item.EndDate).HasColumnType("date").IsRequired();
        builder.Property(item => item.Purpose).HasMaxLength(2000).IsRequired();
        builder.Property(item => item.Notes).HasMaxLength(2000);
        builder.Property(item => item.Status).HasMaxLength(24).IsRequired();
        builder.Property(item => item.CancellationReason).HasMaxLength(500);
        builder.HasIndex(item => item.DelegationNumber).IsUnique();
        builder.HasIndex(item => new { item.EmployeeId, item.StartDate });
        builder.HasIndex(item => new { item.Status, item.EndDate });
        builder.HasIndex(item => item.DelegationTypeId);
        builder.HasOne(item => item.Employee).WithMany().HasForeignKey(item => item.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.DelegationType).WithMany().HasForeignKey(item => item.DelegationTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.CreatedByUser).WithMany().HasForeignKey(item => item.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.UpdatedByUser).WithMany().HasForeignKey(item => item.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.CancelledByUser).WithMany().HasForeignKey(item => item.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
