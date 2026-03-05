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
        var adminUser = await GetOrCreateUserAsync(userManager, "admin@hrms.com", "Admin", "Admin@123456");
        var adminEmployee = await GetOrCreateEmployeeAsync(context, adminUser, "admin@hrms.com", "System", "Administrator", "EMP-2024-0001", "Director", "Engineering");
        
        var arjunUser = await GetOrCreateUserAsync(userManager, "arjun.patel@company.com", "Employee", "Arjun@123");
        var arjunEmployee = await GetOrCreateEmployeeAsync(context, arjunUser, "arjun.patel@company.com", "Arjun", "Patel", "EMP-2024-0002", "Software Engineer", "Engineering");
        
        var poojaUser = await GetOrCreateUserAsync(userManager, "pooja.shah@company.com", "HR", "Pooja@123");
        var poojaEmployee = await GetOrCreateEmployeeAsync(context, poojaUser, "pooja.shah@company.com", "Pooja", "Shah", "EMP-2024-0003", "HR Manager", "Human Resources");

        await context.SaveChangesAsync();

        // Update users with employee IDs
        adminUser.EmployeeId = adminEmployee.Id;
        arjunUser.EmployeeId = arjunEmployee.Id;
        poojaUser.EmployeeId = poojaEmployee.Id;
        await userManager.UpdateAsync(adminUser);
        await userManager.UpdateAsync(arjunUser);
        await userManager.UpdateAsync(poojaUser);

        // Seed comprehensive data
        await AllocateLeaveBalancesForEmployee(context, adminEmployee.Id, "system");
        await AllocateLeaveBalancesForEmployee(context, arjunEmployee.Id, "system");
        await AllocateLeaveBalancesForEmployee(context, poojaEmployee.Id, "system");

        await SeedAttendanceAsync(context, new[] { adminEmployee, arjunEmployee, poojaEmployee });
        await SeedLeavesAsync(context, arjunEmployee, poojaEmployee);
        await SeedPayrollAsync(context, new[] { adminEmployee, arjunEmployee, poojaEmployee });
        await SeedChatAsync(context, new[] { adminEmployee, arjunEmployee, poojaEmployee });
    }

    private static async Task<ApplicationUser> GetOrCreateUserAsync(UserManager<ApplicationUser> userManager, string email, string role, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null) return user;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
        return user;
    }

    private static async Task<Employee> GetOrCreateEmployeeAsync(AppDbContext context, ApplicationUser user, string email, string firstName, string lastName, string code, string designationTitle, string departmentName)
    {
        var employee = await context.Employees.FirstOrDefaultAsync(e => e.Email == email);
        if (employee != null) return employee;

        var department = await context.Departments.FirstAsync(d => d.Name == departmentName);
        var designation = await context.Designations.FirstAsync(d => d.Title == designationTitle);

        employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = code,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = "1234567890",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Gender = Domain.Enums.Gender.Male,
            DepartmentId = department.Id,
            DesignationId = designation.Id,
            EmploymentType = Domain.Enums.EmploymentType.FullTime,
            JoiningDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            UpdatedBy = "system"
        };

        await context.Employees.AddAsync(employee);
        return employee;
    }

    private static async Task SeedAttendanceAsync(AppDbContext context, Employee[] employees)
    {
        if (await context.AttendanceRecords.AnyAsync()) return;

        var today = DateTime.UtcNow.Date;
        var records = new List<AttendanceRecord>();

        foreach (var emp in employees)
        {
            // Seed last 14 days
            for (int i = 1; i <= 14; i++)
            {
                var date = today.AddDays(-i);
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;

                records.Add(new AttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = emp.Id,
                    Date = date,
                    CheckInTime = date.AddHours(9).AddMinutes(new Random().Next(0, 30)),
                    CheckOutTime = date.AddHours(18).AddMinutes(new Random().Next(0, 30)),
                    WorkHours = 9,
                    Status = Domain.Enums.AttendanceStatus.Present,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    UpdatedBy = "system"
                });
            }
        }
        await context.AttendanceRecords.AddRangeAsync(records);
        await context.SaveChangesAsync();
    }

    private static async Task SeedLeavesAsync(AppDbContext context, Employee arjun, Employee pooja)
    {
        if (await context.LeaveApplications.AnyAsync()) return;

        var casualLeave = await context.LeaveTypes.FirstAsync(l => l.Name == "Casual Leave");
        var sickLeave = await context.LeaveTypes.FirstAsync(l => l.Name == "Sick Leave");

        var leaves = new List<LeaveApplication>
        {
            new LeaveApplication
            {
                Id = Guid.NewGuid(),
                EmployeeId = arjun.Id,
                LeaveTypeId = casualLeave.Id,
                StartDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(10), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(12), DateTimeKind.Utc),
                Duration = 3,
                Reason = "Family vacation mapping",
                Status = Domain.Enums.LeaveStatus.Submitted, // Pending
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = arjun.FirstName,
                UpdatedBy = arjun.FirstName
            },
            new LeaveApplication
            {
                Id = Guid.NewGuid(),
                EmployeeId = pooja.Id,
                LeaveTypeId = sickLeave.Id,
                StartDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-5), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-4), DateTimeKind.Utc),
                Duration = 2,
                Reason = "Fever and cold",
                Status = Domain.Enums.LeaveStatus.Approved,
                ApprovedBy = "System Administrator",
                ApprovedAt = DateTime.UtcNow.AddDays(-6),
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-6),
                CreatedBy = pooja.FirstName,
                UpdatedBy = "system"
            }
        };

        await context.LeaveApplications.AddRangeAsync(leaves);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPayrollAsync(AppDbContext context, Employee[] employees)
    {
        if (await context.PayrollRecords.AnyAsync()) return;

        var lastMonth = DateTime.UtcNow.AddMonths(-1);
        var records = new List<PayrollRecord>();

        foreach (var emp in employees)
        {
            records.Add(new PayrollRecord
            {
                Id = Guid.NewGuid(),
                EmployeeId = emp.Id,
                Month = lastMonth.Month,
                Year = lastMonth.Year,
                BasicSalary = 50000,
                HouseRentAllowance = 20000,
                TransportAllowance = 5000,
                MedicalAllowance = 5000,
                SpecialAllowance = 10000,
                GrossSalary = 90000,
                ProvidentFund = 6000,
                ProfessionalTax = 200,
                IncomeTax = 8000,
                OtherDeductions = 0,
                TotalDeductions = 14200,
                NetSalary = 75800,
                WorkingDays = 22,
                PresentDays = 22,
                AbsentDays = 0,
                TotalWorkHours = 198,
                Status = Domain.Enums.PayrollStatus.Paid,
                PaidAt = new DateTime(lastMonth.Year, lastMonth.Month, 28, 0, 0, 0, DateTimeKind.Utc),
                PaidBy = "System",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system",
                UpdatedBy = "system"
            });
        }
        await context.PayrollRecords.AddRangeAsync(records);
        await context.SaveChangesAsync();
    }

    private static async Task SeedChatAsync(AppDbContext context, Employee[] employees)
    {
        if (await context.ChatConversations.AnyAsync()) return;

        var groupChat = new ChatConversation
        {
            Id = Guid.NewGuid(),
            Name = "Engineering & HR Sync",
            Type = Domain.Enums.ChatType.Group,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            UpdatedBy = "system"
        };
        await context.ChatConversations.AddAsync(groupChat);

        foreach (var emp in employees)
        {
            await context.ChatMembers.AddAsync(new ChatMember
            {
                Id = Guid.NewGuid(),
                ConversationId = groupChat.Id,
                EmployeeId = emp.Id,
                IsAdmin = emp.Email == "admin@hrms.com",
                JoinedAt = DateTime.UtcNow.AddDays(-30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system",
                UpdatedBy = "system"
            });
        }

        var messages = new List<ChatMessage>
        {
            new ChatMessage { Id = Guid.NewGuid(), ConversationId = groupChat.Id, SenderEmployeeId = employees[0].Id, Type = Domain.Enums.MessageType.Text, Content = "Welcome to the new HRMS portal everyone!", SentAt = DateTime.UtcNow.AddHours(-24), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new ChatMessage { Id = Guid.NewGuid(), ConversationId = groupChat.Id, SenderEmployeeId = employees[1].Id, Type = Domain.Enums.MessageType.Text, Content = "Looks great! The dashboard is very responsive.", SentAt = DateTime.UtcNow.AddHours(-23), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" },
            new ChatMessage { Id = Guid.NewGuid(), ConversationId = groupChat.Id, SenderEmployeeId = employees[2].Id, Type = Domain.Enums.MessageType.Text, Content = "Please ensure all pending leave requests are submitted by EOD Friday.", SentAt = DateTime.UtcNow.AddHours(-22), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, CreatedBy = "system", UpdatedBy = "system" }
        };

        await context.ChatMessages.AddRangeAsync(messages);
        await context.SaveChangesAsync();
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
