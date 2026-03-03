export interface LeaveRequest {
    id: string;
    employeeId: string;
    employeeName: string;
    leaveTypeId: string;
    leaveTypeName: string;
    startDate: string;
    endDate: string;
    reason: string;
    status: 'Pending' | 'Approved' | 'Rejected' | 'Cancelled';
    appliedDate: string;
    days: number;
}

export interface LeaveBalance {
    leaveTypeId: string;
    leaveTypeName: string;
    totalDays: number;
    usedDays: number;
    remainingDays: number;
}

export interface LeaveType {
    id: string;
    name: string;
    defaultDays: number;
}
