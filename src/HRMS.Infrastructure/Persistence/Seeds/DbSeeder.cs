using HRMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.Infrastructure.Persistence.Seeds;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedDepartmentsAsync(context);
        await SeedDesignationsAsync(context);
        await SeedLeaveTypesAsync(context);
        await SeedAdminUserAsync(userManager, context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "HR", "Employee" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedDepartmentsAsync(AppDbContext context)
    {
        if (await context.Departments.AnyAsync()) return;

        var departments = new List<Department>
        {
            new() { Id = Guid.NewGuid(), Name = "Engineering", Description = "Software Development", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system" , UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Name = "Human Resources", Description = "HR Operations", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Name = "Finance", Description = "Finance and Accounting", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Name = "Marketing", Description = "Marketing and Communications", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Name = "Operations", Description = "Business Operations", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" }
        };

        await context.Departments.AddRangeAsync(departments);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDesignationsAsync(AppDbContext context)
    {
        if (await context.Designations.AnyAsync()) return;

        var designations = new List<Designation>
        {
            new() { Id = Guid.NewGuid(), Title = "Software Engineer", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Title = "Senior Software Engineer", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Title = "Tech Lead", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Title = "HR Manager", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Title = "HR Executive", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Title = "Manager", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Title = "Director", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Title = "Intern", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" }
        };

        await context.Designations.AddRangeAsync(designations);
        await context.SaveChangesAsync();
    }

    private static async Task SeedLeaveTypesAsync(AppDbContext context)
    {
        if (await context.LeaveTypes.AnyAsync()) return;

        var leaveTypes = new List<LeaveType>
        {
            new() { Id = Guid.NewGuid(), Name = "Casual Leave", Description = "Short-notice personal matters", DefaultDays = 12, IsPaid = true, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Name = "Sick Leave", Description = "Medical or health-related absence", DefaultDays = 10, IsPaid = true, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Name = "Earned Leave", Description = "Accrued based on months of service", DefaultDays = 15, IsPaid = true, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Name = "Maternity Leave", Description = "Childbirth and recovery", DefaultDays = 182, IsPaid = true, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Name = "Paternity Leave", Description = "Leave for new fathers", DefaultDays = 15, IsPaid = true, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new() { Id = Guid.NewGuid(), Name = "Unpaid Leave", Description = "Leave without pay", DefaultDays = 0, IsPaid = false, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" }
        };

        await context.LeaveTypes.AddRangeAsync(leaveTypes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        var adminEmail = "admin@hrms.com";
        var existingUser = await userManager.FindByEmailAsync(adminEmail);

        if (existingUser != null) return;

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123456");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");

            var department = await context.Departments.FirstAsync(d => d.Name == "Engineering");
            var designation = await context.Designations.FirstAsync(d => d.Title == "Director");

            var adminEmployee = new Employee
            {
                Id = Guid.NewGuid(),
                EmployeeCode = "EMP-2024-0001",
                FirstName = "System",
                LastName = "Administrator",
                Email = adminEmail,
                Phone = "0000000000",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = Domain.Enums.Gender.Male,
                DepartmentId = department.Id,
                DesignationId = designation.Id,
                EmploymentType = Domain.Enums.EmploymentType.FullTime,
                JoiningDate = new DateTime(2024, 1, 1),
                IsActive = true,
                UserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system",
                UpdatedBy = "system"
            };

            await context.Employees.AddAsync(adminEmployee);
            await context.SaveChangesAsync();
            
            // Update user with employee ID after saving employee
            adminUser.EmployeeId = adminEmployee.Id;
            await userManager.UpdateAsync(adminUser);
            
            await CreateTestEmployeeAsync(userManager, context, "arjun.patel@company.com", "Arjun", "Patel", "EMP-2024-0002");
        }
    }

    private static async Task CreateTestEmployeeAsync(UserManager<ApplicationUser> userManager, AppDbContext context, string email, string firstName, string lastName, string employeeCode)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null) return;
        
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, "Password@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Employee");

            var department = await context.Departments.FirstAsync(d => d.Name == "Engineering");
            var designation = await context.Designations.FirstAsync(d => d.Title == "Software Engineer");

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                EmployeeCode = employeeCode,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = "1234567890",
                DateOfBirth = new DateTime(1995, 5, 15),
                Gender = Domain.Enums.Gender.Male,
                DepartmentId = department.Id,
                DesignationId = designation.Id,
                EmploymentType = Domain.Enums.EmploymentType.FullTime,
                JoiningDate = new DateTime(2024, 1, 1),
                IsActive = true,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system",
                UpdatedBy = "system"
            };

            await context.Employees.AddAsync(employee);
            await context.SaveChangesAsync();
            
            user.EmployeeId = employee.Id;
            await userManager.UpdateAsync(user);
            
            await AllocateLeaveBalancesForEmployee(context, employee.Id, "system");
        }
    }
    
    private static async Task AllocateLeaveBalancesForEmployee(AppDbContext context, Guid employeeId, string performedBy)
    {
        var leaveTypes = await context.LeaveTypes.Where(lt => !lt.IsDeleted).ToListAsync();
        var year = DateTime.UtcNow.Year;
        
        foreach (var lt in leaveTypes)
        {
            var existing = await context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.LeaveTypeId == lt.Id && lb.Year == year);
                
            if (existing != null) continue;
            
            var allocation = lt.DefaultDays;
            
            var balance = new LeaveBalance
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                LeaveTypeId = lt.Id,
                Year = year,
                TotalAllocated = allocation,
                Used = 0,
                Remaining = allocation,
                CreatedBy = performedBy,
                UpdatedBy = performedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            await context.LeaveBalances.AddAsync(balance);
        }
        
        await context.SaveChangesAsync();
    }
}
