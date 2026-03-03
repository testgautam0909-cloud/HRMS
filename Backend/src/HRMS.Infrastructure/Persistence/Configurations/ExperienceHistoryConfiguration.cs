using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class ExperienceHistoryConfiguration : IEntityTypeConfiguration<ExperienceHistory>
{
    public void Configure(EntityTypeBuilder<ExperienceHistory> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Designation)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.HasOne(e => e.Employee)
            .WithMany(emp => emp.ExperienceHistories)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
