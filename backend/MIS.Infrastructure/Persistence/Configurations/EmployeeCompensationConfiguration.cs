using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class EmployeeCompensationConfiguration : IEntityTypeConfiguration<EmployeeCompensation>
{
    public void Configure(EntityTypeBuilder<EmployeeCompensation> builder)
    {
        builder.ToTable("EmployeeCompensations", table =>
        {
            table.HasCheckConstraint("CK_EmployeeCompensations_Amounts", "\"BasicSalary\" >= 0 AND \"Allowances\" >= 0");
            table.HasCheckConstraint("CK_EmployeeCompensations_DateRange", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" >= \"EffectiveFrom\"");
            table.HasCheckConstraint("CK_EmployeeCompensations_Current", "NOT \"IsCurrent\" OR \"EffectiveTo\" IS NULL");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BasicSalary).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Allowances).HasPrecision(18, 2).IsRequired();
        builder.Ignore(x => x.TotalSalary);
        builder.Property(x => x.EffectiveFrom).HasColumnType("date").IsRequired();
        builder.Property(x => x.EffectiveTo).HasColumnType("date");
        builder.Property(x => x.IsCurrent).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.BankName).HasMaxLength(160);
        builder.Property(x => x.BankAccountNumber).HasMaxLength(100);
        builder.Property(x => x.Iban).HasMaxLength(64);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => new { x.EmployeeId, x.EffectiveFrom });
        builder.HasIndex(x => x.EmployeeId).IsUnique().HasFilter("\"IsCurrent\" = TRUE");
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
    }
}
