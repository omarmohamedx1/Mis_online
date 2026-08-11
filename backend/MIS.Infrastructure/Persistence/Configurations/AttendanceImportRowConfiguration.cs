using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class AttendanceImportRowConfiguration : IEntityTypeConfiguration<AttendanceImportRow>
{
    public void Configure(EntityTypeBuilder<AttendanceImportRow> builder)
    {
        builder.ToTable("AttendanceImportRows", table =>
        {
            table.HasCheckConstraint("CK_AttendanceImportRows_CheckTimes", "\"CheckOut\" IS NULL OR \"CheckIn\" IS NULL OR \"CheckOut\" >= \"CheckIn\"");
            table.HasCheckConstraint("CK_AttendanceImportRows_CanImport", "NOT \"CanImport\" OR (\"EmployeeId\" IS NOT NULL AND \"AttendanceDate\" IS NOT NULL)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceRowNumbersJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.SourceRowsJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.SourceEmployeeNumber).HasMaxLength(100);
        builder.Property(x => x.SourceEmployeeName).HasMaxLength(200);
        builder.Property(x => x.AttendanceDate).HasColumnType("date");
        builder.Property(x => x.PunchesJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CategoriesJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ErrorsJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.CanImport).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => new { x.BatchId, x.CanImport });
        builder.HasIndex(x => new { x.BatchId, x.AttendanceDate });
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.SourceEmployeeNumber);
        builder.HasOne(x => x.Batch).WithMany().HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
    }
}
