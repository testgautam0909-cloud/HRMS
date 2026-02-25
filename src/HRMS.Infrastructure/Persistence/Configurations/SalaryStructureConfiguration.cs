using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class SalaryStructureConfiguration : IEntityTypeConfiguration<SalaryStructure>
{
    public void Configure(EntityTypeBuilder<SalaryStructure> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => new { s.EmployeeId, s.IsActive });

        builder.Property(s => s.BasicSalary).HasPrecision(18, 2);
        builder.Property(s => s.HouseRentAllowance).HasPrecision(18, 2);
        builder.Property(s => s.TransportAllowance).HasPrecision(18, 2);
        builder.Property(s => s.MedicalAllowance).HasPrecision(18, 2);
        builder.Property(s => s.SpecialAllowance).HasPrecision(18, 2);
        builder.Property(s => s.GrossSalary).HasPrecision(18, 2);
        builder.Property(s => s.ProvidentFund).HasPrecision(18, 2);
        builder.Property(s => s.ProfessionalTax).HasPrecision(18, 2);
        builder.Property(s => s.IncomeTax).HasPrecision(18, 2);
        builder.Property(s => s.OtherDeductions).HasPrecision(18, 2);
        builder.Property(s => s.TotalDeductions).HasPrecision(18, 2);
        builder.Property(s => s.NetSalary).HasPrecision(18, 2);
        builder.Property(s => s.CTC).HasPrecision(18, 2);

        builder.HasOne(s => s.Employee)
            .WithMany(e => e.SalaryStructures)
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.IncrementRequest)
            .WithOne(i => i.NewSalaryStructure)
            .HasForeignKey<SalaryStructure>(s => s.IncrementRequestId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
