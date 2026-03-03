using Microsoft.AspNetCore.Identity;

namespace HRMS.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
