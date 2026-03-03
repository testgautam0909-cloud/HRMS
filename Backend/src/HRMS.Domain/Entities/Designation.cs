namespace HRMS.Domain.Entities;

public class Designation : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
