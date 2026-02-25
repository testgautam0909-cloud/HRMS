using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class PayrollRecordConfiguration : IEntityTypeConfiguration<PayrollRecord>
{
    public void Configure(EntityTypeBuilder<PayrollRecord> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasIndex(p => new { p.EmployeeId, p.Month, p.Year })
            .IsUnique();

        builder.Property(p => p.BasicSalary).HasPrecision(18, 2);
        builder.Property(p => p.HouseRentAllowance).HasPrecision(18, 2);
        builder.Property(p => p.TransportAllowance).HasPrecision(18, 2);
        builder.Property(p => p.MedicalAllowance).HasPrecision(18, 2);
        builder.Property(p => p.SpecialAllowance).HasPrecision(18, 2);
        builder.Property(p => p.GrossSalary).HasPrecision(18, 2);
        builder.Property(p => p.ProvidentFund).HasPrecision(18, 2);
        builder.Property(p => p.ProfessionalTax).HasPrecision(18, 2);
        builder.Property(p => p.IncomeTax).HasPrecision(18, 2);
        builder.Property(p => p.OtherDeductions).HasPrecision(18, 2);
        builder.Property(p => p.AttendanceDeduction).HasPrecision(18, 2);
        builder.Property(p => p.UnpaidLeaveDeduction).HasPrecision(18, 2);
        builder.Property(p => p.BonusAmount).HasPrecision(18, 2);
        builder.Property(p => p.TotalDeductions).HasPrecision(18, 2);
        builder.Property(p => p.NetSalary).HasPrecision(18, 2);
        builder.Property(p => p.TotalWorkHours).HasPrecision(18, 2);

        builder.Property(p => p.Status)
            .HasConversion<short>();

        builder.Property(p => p.OverrideReason)
            .HasMaxLength(1000);

        builder.Property(p => p.OverriddenBy)
            .HasMaxLength(256);

        builder.Property(p => p.LockedBy)
            .HasMaxLength(256);

        builder.Property(p => p.PaidBy)
            .HasMaxLength(256);

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        builder.HasOne(p => p.Employee)
            .WithMany(e => e.PayrollRecords)
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
