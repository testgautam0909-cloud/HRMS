using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Domain.Interfaces.Repositories;

public interface IDocumentRepository : IGenericRepository<EmployeeDocument>
{
    Task<IEnumerable<EmployeeDocument>> GetByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<EmployeeDocument>> GetByEmployeeAndCategoryAsync(Guid employeeId, DocumentCategory category);
}
