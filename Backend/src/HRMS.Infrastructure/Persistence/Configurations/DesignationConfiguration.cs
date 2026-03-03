using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
{
    public void Configure(EntityTypeBuilder<Designation> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(d => d.Title)
            .IsUnique();

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
