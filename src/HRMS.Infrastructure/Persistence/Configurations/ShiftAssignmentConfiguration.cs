using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class ShiftAssignmentConfiguration : IEntityTypeConfiguration<ShiftAssignment>
{
    public void Configure(EntityTypeBuilder<ShiftAssignment> builder)
    {
        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.AssignmentDate)
            .HasColumnType("timestamp with time zone");

        builder.Property(sa => sa.EndDate)
            .HasColumnType("timestamp with time zone");

        builder.HasOne(sa => sa.Employee)
            .WithMany(e => e.ShiftAssignments)
            .HasForeignKey(sa => sa.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sa => sa.Shift)
            .WithMany(s => s.ShiftAssignments)
            .HasForeignKey(sa => sa.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sa => new { sa.EmployeeId, sa.AssignmentDate })
            .IsUnique();

        builder.HasQueryFilter(sa => !sa.IsDeleted);
    }
}