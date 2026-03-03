using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class SalarySlipConfiguration : IEntityTypeConfiguration<SalarySlip>
{
    public void Configure(EntityTypeBuilder<SalarySlip> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.CloudinaryPublicId)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.SecureUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(s => s.FileName)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasOne(s => s.PayrollRecord)
            .WithOne(p => p.SalarySlip)
            .HasForeignKey<SalarySlip>(s => s.PayrollRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
