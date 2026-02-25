using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class IncrementRequestConfiguration : IEntityTypeConfiguration<IncrementRequest>
{
    public void Configure(EntityTypeBuilder<IncrementRequest> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.CurrentCTC).HasPrecision(18, 2);
        builder.Property(i => i.IncrementPercentage).HasPrecision(18, 2);
        builder.Property(i => i.NewCTC).HasPrecision(18, 2);

        builder.Property(i => i.Justification)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(i => i.Status)
            .HasConversion<short>();

        builder.Property(i => i.Remarks)
            .HasMaxLength(1000);

        builder.Property(i => i.ApprovedBy)
            .HasMaxLength(256);

        builder.HasOne(i => i.Employee)
            .WithMany(e => e.IncrementRequests)
            .HasForeignKey(i => i.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}
