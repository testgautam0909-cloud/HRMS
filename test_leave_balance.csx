using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HRMS.Infrastructure.Persistence;
using HRMS.Application.Services;

// Create service collection
var services = new ServiceCollection();
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=hrms_db;Username=postgres;Password=postgres"));

// Add other required services
services.AddScoped<ILeaveService, LeaveService>();
services.AddAutoMapper(typeof(Program).Assembly);

var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();
var leaveService = scope.ServiceProvider.GetRequiredService<ILeaveService>();

try 
{
    var employeeId = Guid.Parse("8e2ddfca-8413-4f3c-b293-48dfcd48d458");
    var year = 2026;
    var performedBy = "system";

    Console.WriteLine($"Allocating leave balances for employee {employeeId} for year {year}");
    
    // Call the AllocateLeaveBalancesAsync method
    await ((LeaveService)leaveService).AllocateLeaveBalancesAsync(employeeId, year, performedBy);
    
    Console.WriteLine("Leave balances allocated successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }
}