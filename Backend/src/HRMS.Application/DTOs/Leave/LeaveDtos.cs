using HRMS.Domain.Enums;

namespace HRMS.Application.DTOs.Leave;

public class LeaveApplyDto
{
    public Guid LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Duration { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class LeaveUpdateDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Duration { get; set; }
    public string? Reason { get; set; }
}

public class LeaveApproveRejectDto
{
    public bool IsApproved { get; set; }
    public string? Remarks { get; set; }
}

public class LeaveResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public Guid LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Duration { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LeaveBalanceDto
{
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalAllocated { get; set; }
    public decimal Used { get; set; }
    public decimal Remaining { get; set; }
}

public class LeaveTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultDays { get; set; }
    public bool IsPaid { get; set; }
    public bool IsActive { get; set; }
}

public class LeaveTypeCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultDays { get; set; }
    public bool IsPaid { get; set; } = true;
}
