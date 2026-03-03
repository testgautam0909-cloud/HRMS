using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces.Repositories;

public interface ISalaryRepository : IGenericRepository<SalaryStructure>
{
    Task<SalaryStructure?> GetActiveStructureAsync(Guid employeeId);
    Task<IEnumerable<SalaryStructure>> GetHistoryAsync(Guid employeeId);
    Task<bool> HasOverlappingStructureAsync(Guid employeeId, DateTime effectiveDate, Guid? excludeId = null);
}

public interface IIncrementRepository : IGenericRepository<IncrementRequest>
{
    Task<IEnumerable<IncrementRequest>> GetByEmployeeAsync(Guid employeeId);
    Task<(IEnumerable<IncrementRequest> Items, int TotalCount)> GetIncrementsPagedAsync(
        Guid? employeeId, int page, int pageSize);
}
