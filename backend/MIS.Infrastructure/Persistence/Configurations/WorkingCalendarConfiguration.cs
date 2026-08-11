using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MIS.Domain.Entities;

namespace MIS.Infrastructure.Persistence.Configurations;

public sealed class WorkingCalendarConfiguration : IEntityTypeConfiguration<WorkingCalendar>
{
    public void Configure(EntityTypeBuilder<WorkingCalendar> builder)
    {
        builder.ToTable("WorkingCalendars");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.TimeZoneId);
        builder.HasMany(x => x.Days)
            .WithOne(x => x.WorkingCalendar)
            .HasForeignKey(x => x.WorkingCalendarId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(x => x.Days).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
