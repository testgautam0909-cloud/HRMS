export interface PayrollRecord {
    id: string;
    employeeId: string;
    employeeName: string;
    employeeCode: string;
    month: number;
    year: number;
    monthName?: string;
    basicSalary: number;
    houseRentAllowance: number;
    transportAllowance: number;
    medicalAllowance: number;
    specialAllowance: number;
    grossSalary: number;
    providentFund: number;
    professionalTax: number;
    incomeTax: number;
    otherDeductions: number;
    attendanceDeduction: number;
    unpaidLeaveDeduction: number;
    bonusAmount: number;
    totalDeductions: number;
    netSalary: number;
    workingDays: number;
    presentDays: number;
    absentDays: number;
    totalWorkHours: number;
    status: string;
    isLocked: boolean;
    salarySlipUrl?: string;
    createdAt: string;
}

export interface PayrollSummary {
    month: number;
    year: number;
    totalEmployees: number;
    totalGrossSalary: number;
    totalDeductions: number;
    totalNetSalary: number;
    totalPaid: number;
    totalPending: number;
}

export interface PayrollGenerateResult {
    generated: number;
    skipped: number;
}

export interface PayrollOverrideRequest {
    bonusAmount?: number;
    penaltyAmount?: number;
    overrideReason?: string;
}
