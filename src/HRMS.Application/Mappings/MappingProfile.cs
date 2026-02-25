using AutoMapper;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.DTOs.Salary;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.DTOs.Document;
using HRMS.Application.DTOs.Chat;
using HRMS.Application.DTOs.Audit;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<EmployeeCreateDto, Employee>();
        CreateMap<Employee, EmployeeResponseDto>()
            .ForMember(d => d.Department, o => o.MapFrom(s => s.Department.Name))
            .ForMember(d => d.Designation, o => o.MapFrom(s => s.Designation.Title))
            .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender.ToString()))
            .ForMember(d => d.EmploymentType, o => o.MapFrom(s => s.EmploymentType.ToString()));
        CreateMap<Employee, EmployeeSummaryDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.Department, o => o.MapFrom(s => s.Department.Name))
            .ForMember(d => d.Designation, o => o.MapFrom(s => s.Designation.Title));
        CreateMap<Employee, EmployeeProfileDto>()
            .ForMember(d => d.Department, o => o.MapFrom(s => s.Department.Name))
            .ForMember(d => d.Designation, o => o.MapFrom(s => s.Designation.Title))
            .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender.ToString()))
            .ForMember(d => d.EmploymentType, o => o.MapFrom(s => s.EmploymentType.ToString()));

        CreateMap<ExperienceHistory, ExperienceHistoryDto>();
        CreateMap<ExperienceHistoryCreateDto, ExperienceHistory>();

        CreateMap<Department, DepartmentDto>();
        CreateMap<DepartmentCreateDto, Department>();
        CreateMap<Designation, DesignationDto>();
        CreateMap<DesignationCreateDto, Designation>();

        CreateMap<AttendanceRecord, AttendanceResponseDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<LeaveApplication, LeaveResponseDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"))
            .ForMember(d => d.LeaveTypeName, o => o.MapFrom(s => s.LeaveType.Name))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
        CreateMap<LeaveBalance, LeaveBalanceDto>()
            .ForMember(d => d.LeaveTypeName, o => o.MapFrom(s => s.LeaveType.Name));
        CreateMap<LeaveType, LeaveTypeDto>();
        CreateMap<LeaveTypeCreateDto, LeaveType>();

        CreateMap<SalaryStructure, SalaryStructureResponseDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"));
        CreateMap<IncrementRequest, IncrementResponseDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<PayrollRecord, PayrollResponseDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"))
            .ForMember(d => d.EmployeeCode, o => o.MapFrom(s => s.Employee.EmployeeCode))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.SalarySlipUrl, o => o.MapFrom(s => s.SalarySlip != null ? s.SalarySlip.SecureUrl : null));

        CreateMap<EmployeeDocument, DocumentResponseDto>()
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category.ToString()));

        CreateMap<ChatConversation, ConversationResponseDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));
        CreateMap<ChatMember, ChatMemberDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => $"{s.Employee.FirstName} {s.Employee.LastName}"));
        CreateMap<ChatMessage, MessageResponseDto>()
            .ForMember(d => d.SenderName, o => o.MapFrom(s => $"{s.Sender.FirstName} {s.Sender.LastName}"))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<AuditLog, AuditLogResponseDto>()
            .ForMember(d => d.Action, o => o.MapFrom(s => s.Action.ToString()));
    }
}
