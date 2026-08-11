using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Constants;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class AttendanceImportBatchConfiguration : IEntityTypeConfiguration<AttendanceImportBatch>
{
    public void Configure(EntityTypeBuilder<AttendanceImportBatch> builder)
    {
        builder.ToTable("AttendanceImportBatches", table =>
        {
            table.HasCheckConstraint("CK_AttendanceImportBatches_FileSize", "\"FileSize\" > 0");
            table.HasCheckConstraint("CK_AttendanceImportBatches_Status", $"\"Status\" IN ('{AttendanceValues.UploadedBatchStatus}', '{AttendanceValues.PreviewReadyBatchStatus}', '{AttendanceValues.ConfirmedBatchStatus}', '{AttendanceValues.FailedBatchStatus}', '{AttendanceValues.CancelledBatchStatus}')");
            table.HasCheckConstraint("CK_AttendanceImportBatches_Counts", "\"TotalRows\" >= 0 AND \"ValidRows\" >= 0 AND \"InvalidRows\" >= 0 AND \"EmployeeNotFoundRows\" >= 0 AND \"DuplicateRows\" >= 0 AND \"MissingCheckInRows\" >= 0 AND \"MissingCheckOutRows\" >= 0 AND \"ImportedRecords\" >= 0");
            table.HasCheckConstraint("CK_AttendanceImportBatches_ImportedCount", "\"ImportedRecords\" <= \"ValidRows\"");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(127);
        builder.Property(x => x.FileSize).IsRequired();
        builder.Property(x => x.FileHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.StorageKey).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.MappingJson).HasColumnType("jsonb");
        builder.Property(x => x.FailureReason).HasMaxLength(2000);
        builder.Property(x => x.UploadedAt).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.HasIndex(x => x.StorageKey).IsUnique();
        builder.HasIndex(x => x.FileHash, "IX_AttendanceImportBatches_FileHash");
        builder.HasIndex(x => x.FileHash, "UX_AttendanceImportBatches_ConfirmedFileHash")
            .IsUnique()
            .HasFilter($"\"Status\" = '{AttendanceValues.ConfirmedBatchStatus}'");
        builder.HasIndex(x => new { x.Status, x.UploadedAt });
        builder.HasIndex(x => new { x.UploadedByUserId, x.UploadedAt });
        builder.HasOne(x => x.UploadedByUser).WithMany().HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
