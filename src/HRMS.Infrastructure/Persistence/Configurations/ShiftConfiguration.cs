using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.BreakStartTime)
            .IsRequired(false);

        builder.Property(s => s.BreakEndTime)
            .IsRequired(false);

        builder.HasIndex(s => s.IsDefault)
            .IsUnique()
            .HasFilter("\"IsDefault\" = true");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}