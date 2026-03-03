using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.HasKey(lb => lb.Id);

        builder.HasIndex(lb => new { lb.EmployeeId, lb.LeaveTypeId, lb.Year })
            .IsUnique();

        builder.Property(lb => lb.TotalAllocated)
            .HasPrecision(18, 2);

        builder.Property(lb => lb.Used)
            .HasPrecision(18, 2);

        builder.Property(lb => lb.Remaining)
            .HasPrecision(18, 2);

        builder.Property(lb => lb.RowVersion)
            .IsRowVersion();

        builder.HasOne(lb => lb.Employee)
            .WithMany(e => e.LeaveBalances)
            .HasForeignKey(lb => lb.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lb => lb.LeaveType)
            .WithMany(lt => lt.LeaveBalances)
            .HasForeignKey(lb => lb.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(lb => !lb.IsDeleted);
    }
}
