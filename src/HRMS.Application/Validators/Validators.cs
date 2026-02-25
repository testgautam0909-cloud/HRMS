using FluentValidation;
using HRMS.Application.DTOs.Auth;
using HRMS.Application.DTOs.Employee;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.DTOs.Salary;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.DTOs.Document;
using HRMS.Application.DTOs.Chat;

namespace HRMS.Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DateOfBirth).NotEmpty()
            .LessThan(DateTime.UtcNow.AddYears(-18)).WithMessage("Employee must be at least 18 years old.");
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.DesignationId).NotEmpty();
        RuleFor(x => x.Role).NotEmpty().Must(r => r == "Admin" || r == "HR" || r == "Employee");
    }
}

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}

public class EmployeeCreateDtoValidator : AbstractValidator<EmployeeCreateDto>
{
    public EmployeeCreateDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DateOfBirth).NotEmpty()
            .LessThan(DateTime.UtcNow.AddYears(-18)).WithMessage("Employee must be at least 18 years old.");
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.DesignationId).NotEmpty();
    }
}

public class CheckInDtoValidator : AbstractValidator<CheckInDto>
{
    public CheckInDtoValidator()
    {
        RuleFor(x => x.Date).NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Cannot check in for a future date.");
    }
}

public class LeaveApplyDtoValidator : AbstractValidator<LeaveApplyDto>
{
    public LeaveApplyDtoValidator()
    {
        RuleFor(x => x.LeaveTypeId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty()
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date.AddDays(-3)).WithMessage("Start date must not be more than 3 days in the past.");
        RuleFor(x => x.EndDate).NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be on or after start date.");
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0.5m);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public class SalaryStructureCreateDtoValidator : AbstractValidator<SalaryStructureCreateDto>
{
    public SalaryStructureCreateDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.BasicSalary).GreaterThan(0);
        RuleFor(x => x.HouseRentAllowance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TransportAllowance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MedicalAllowance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SpecialAllowance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ProvidentFund).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ProfessionalTax).GreaterThanOrEqualTo(0);
        RuleFor(x => x.IncomeTax).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OtherDeductions).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EffectiveDate).NotEmpty();
    }
}

public class IncrementRequestDtoValidator : AbstractValidator<IncrementRequestDto>
{
    public IncrementRequestDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.IncrementPercentage).InclusiveBetween(0.01m, 100m);
        RuleFor(x => x.Justification).NotEmpty().MinimumLength(20).MaximumLength(2000);
        RuleFor(x => x.RequestDate).NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Request date cannot be in the future.");
    }
}

public class PayrollGenerateDtoValidator : AbstractValidator<PayrollGenerateDto>
{
    public PayrollGenerateDtoValidator()
    {
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).GreaterThanOrEqualTo(DateTime.UtcNow.Year - 1);
    }
}

public class MessageSendDtoValidator : AbstractValidator<MessageSendDto>
{
    public MessageSendDtoValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.Content).MaximumLength(4000);
        RuleFor(x => x).Must(x => !string.IsNullOrWhiteSpace(x.Content) || !string.IsNullOrWhiteSpace(x.FileUrl))
            .WithMessage("Message must have either text content or a file.");
    }
}

public class DocumentUploadDtoValidator : AbstractValidator<DocumentUploadDto>
{
    public DocumentUploadDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.File).NotNull();
        RuleFor(x => x.File.Length).LessThanOrEqualTo(10 * 1024 * 1024)
            .WithMessage("File size must not exceed 10 MB.")
            .When(x => x.File != null);
        RuleFor(x => x.File.ContentType)
            .Must(ct => ct == "application/pdf" || ct == "image/jpeg" || ct == "image/png" || ct == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            .WithMessage("Allowed file types are PDF, JPEG, PNG, and DOCX.")
            .When(x => x.File != null);
    }
}

public class ConversationCreateDtoValidator : AbstractValidator<ConversationCreateDto>
{
    public ConversationCreateDtoValidator()
    {
        RuleFor(x => x.MemberIds).NotEmpty()
            .Must(m => m.Count <= 100).WithMessage("A group can have at most 100 members.");
        RuleFor(x => x.Name).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
