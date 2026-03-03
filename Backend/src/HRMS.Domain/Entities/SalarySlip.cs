namespace HRMS.Domain.Entities;

public class SalarySlip : BaseEntity
{
    public Guid PayrollRecordId { get; set; }
    public PayrollRecord PayrollRecord { get; set; } = null!;
    public string CloudinaryPublicId { get; set; } = string.Empty;
    public string SecureUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
