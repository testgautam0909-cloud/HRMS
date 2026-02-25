using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => new { a.EntityName, a.EntityId });

        builder.HasIndex(a => new { a.PerformedBy, a.PerformedAt });

        builder.HasIndex(a => a.PerformedAt);

        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Action)
            .HasConversion<short>();

        builder.Property(a => a.PerformedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.Remarks)
            .HasMaxLength(1000);

        builder.ToTable(t => t.HasCheckConstraint("CK_audit_logs_no_update", "true"));
    }
}
