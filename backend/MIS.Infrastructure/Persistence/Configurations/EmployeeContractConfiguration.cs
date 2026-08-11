using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class EmployeeContractConfiguration : IEntityTypeConfiguration<EmployeeContract>
{
    public void Configure(EntityTypeBuilder<EmployeeContract> builder)
    {
        builder.ToTable("EmployeeContracts", table =>
        {
            table.HasCheckConstraint("CK_EmployeeContracts_DateRange", "\"ContractEndDate\" IS NULL OR \"ContractEndDate\" >= \"ContractStartDate\"");
            table.HasCheckConstraint("CK_EmployeeContracts_ProbationDateRange", "\"ProbationEndDate\" IS NULL OR (\"ProbationStartDate\" IS NOT NULL AND \"ProbationEndDate\" >= \"ProbationStartDate\")");
            table.HasCheckConstraint("CK_EmployeeContracts_Status", $"\"Status\" IN ('{EmployeeContract.DraftStatus}', '{EmployeeContract.ActiveStatus}', '{EmployeeContract.ExpiredStatus}', '{EmployeeContract.TerminatedStatus}')");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ContractStartDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.ContractEndDate).HasColumnType("date");
        builder.Property(x => x.ProbationStartDate).HasColumnType("date");
        builder.Property(x => x.ProbationEndDate).HasColumnType("date");
        builder.Property(x => x.Status).HasMaxLength(24).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => new { x.EmployeeId, x.ContractStartDate });
        builder.HasIndex(x => x.EmployeeId)
            .IsUnique()
            .HasFilter($"\"Status\" = '{EmployeeContract.ActiveStatus}'");
        builder.HasIndex(x => x.ContractTypeId);
        builder.HasIndex(x => new { x.Status, x.ContractEndDate });
        builder.HasIndex(x => x.ProbationEndDate);
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ContractType).WithMany().HasForeignKey(x => x.ContractTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}
