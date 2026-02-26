using System.Linq.Expressions;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces.Repositories;
using HRMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence.Repositories;

public class ShiftRepository : GenericRepository<Shift>, IGenericRepository<Shift>
{
    public ShiftRepository(AppDbContext context) : base(context)
    {
    }
}