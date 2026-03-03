using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence.Repositories;

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode)
    {
        return await _dbSet
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
    }

    public async Task<Employee?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Email == email);
    }

    public async Task<Employee?> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.UserId == userId);
    }

    public async Task<int> GetNextSequenceForYearAsync(int year)
    {
        var maxCode = await _dbSet
            .IgnoreQueryFilters()
            .Where(e => e.EmployeeCode.StartsWith($"EMP-{year}-"))
            .OrderByDescending(e => e.EmployeeCode)
            .Select(e => e.EmployeeCode)
            .FirstOrDefaultAsync();

        if (maxCode == null) return 1;

        var parts = maxCode.Split('-');
        if (parts.Length == 3 && int.TryParse(parts[2], out var seq))
            return seq + 1;

        return 1;
    }

    public async Task<Employee?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.ExperienceHistories)
            .Include(e => e.Documents)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<(IEnumerable<Employee> Items, int TotalCount)> GetEmployeesPagedAsync(
        string? searchTerm, Guid? departmentId, bool? isActive, int page, int pageSize)
    {
        var query = _dbSet
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term) ||
                e.EmployeeCode.ToLower().Contains(term));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(e => e.DepartmentId == departmentId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(e => e.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
