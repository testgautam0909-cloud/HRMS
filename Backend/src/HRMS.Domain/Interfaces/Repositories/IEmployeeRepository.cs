using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces.Repositories;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
    Task<Employee?> GetByEmployeeCodeAsync(string employeeCode);
    Task<Employee?> GetByEmailAsync(string email);
    Task<Employee?> GetByUserIdAsync(string userId);
    Task<int> GetNextSequenceForYearAsync(int year);
    Task<Employee?> GetWithDetailsAsync(Guid id);
    Task<(IEnumerable<Employee> Items, int TotalCount)> GetEmployeesPagedAsync(
        string? searchTerm, Guid? departmentId, bool? isActive, int page, int pageSize);
}
