using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => new { a.EmployeeId, a.Date })
            .IsUnique();

        builder.Property(a => a.WorkHours)
            .HasPrecision(18, 2);

        builder.Property(a => a.Status)
            .HasConversion<short>();

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.CorrectionNote)
            .HasMaxLength(500);

        builder.Property(a => a.CorrectedBy)
            .HasMaxLength(256);

        builder.HasOne(a => a.Employee)
            .WithMany(e => e.AttendanceRecords)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
