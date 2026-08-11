using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class HrAuditLogConfiguration : IEntityTypeConfiguration<HrAuditLog>
{
    public void Configure(EntityTypeBuilder<HrAuditLog> builder)
    {
        builder.ToTable("HrAuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OldValue).HasColumnType("jsonb");
        builder.Property(x => x.NewValue).HasColumnType("jsonb");
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Timestamp).IsRequired();
        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => new { x.EmployeeId, x.Timestamp });
        builder.HasIndex(x => new { x.UserId, x.Timestamp });
        builder.HasIndex(x => x.Action);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
    }
}
