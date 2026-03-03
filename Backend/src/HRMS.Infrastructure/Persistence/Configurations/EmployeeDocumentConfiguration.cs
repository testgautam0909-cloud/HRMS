using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.HasKey(d => d.Id);

        builder.HasIndex(d => new { d.EmployeeId, d.Category });

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(d => d.FileType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.CloudinaryPublicId)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.SecureUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(d => d.Category)
            .HasConversion<short>();

        builder.Property(d => d.UploadedBy)
            .HasMaxLength(256);

        builder.HasOne(d => d.Employee)
            .WithMany(e => e.Documents)
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
