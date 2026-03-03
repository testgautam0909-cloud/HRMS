using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class LeaveApplicationConfiguration : IEntityTypeConfiguration<LeaveApplication>
{
    public void Configure(EntityTypeBuilder<LeaveApplication> builder)
    {
        builder.HasKey(la => la.Id);

        builder.HasIndex(la => new { la.EmployeeId, la.Status });

        builder.HasIndex(la => new { la.StartDate, la.EndDate });

        builder.Property(la => la.Duration)
            .HasPrecision(18, 2);

        builder.Property(la => la.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(la => la.Status)
            .HasConversion<short>();

        builder.Property(la => la.RejectionReason)
            .HasMaxLength(500);

        builder.Property(la => la.Remarks)
            .HasMaxLength(500);

        builder.Property(la => la.ApprovedBy)
            .HasMaxLength(256);

        builder.HasOne(la => la.Employee)
            .WithMany(e => e.LeaveApplications)
            .HasForeignKey(la => la.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(la => la.LeaveType)
            .WithMany(lt => lt.LeaveApplications)
            .HasForeignKey(la => la.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(la => !la.IsDeleted);
    }
}
