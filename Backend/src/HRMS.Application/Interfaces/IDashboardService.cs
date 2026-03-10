using HRMS.Application.DTOs.Dashboard;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    }
}
