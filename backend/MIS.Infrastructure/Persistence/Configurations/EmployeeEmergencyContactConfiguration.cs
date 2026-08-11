using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class EmployeeEmergencyContactConfiguration : IEntityTypeConfiguration<EmployeeEmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmployeeEmergencyContact> builder)
    {
        builder.ToTable("EmployeeEmergencyContacts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ContactName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Relationship).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MobileNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.AlternativeNumber).HasMaxLength(32);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.IsPrimary).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.EmployeeId, "IX_EmployeeEmergencyContacts_EmployeeId");
        builder.HasIndex(x => x.EmployeeId, "UX_EmployeeEmergencyContacts_Primary")
            .IsUnique()
            .HasFilter("\"IsPrimary\" = TRUE");
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
    }
}
