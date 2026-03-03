using System.Linq.Expressions;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces.Repositories;
using HRMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence.Repositories;

public class ShiftAssignmentRepository : GenericRepository<ShiftAssignment>, IGenericRepository<ShiftAssignment>
{
    public ShiftAssignmentRepository(AppDbContext context) : base(context)
    {
    }
}