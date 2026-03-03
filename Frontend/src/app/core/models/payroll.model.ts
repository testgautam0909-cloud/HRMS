export interface PayrollRecord {
    id: string;
    employeeId: string;
    employeeName: string;
    month: number;
    year: number;
    monthName: string;
    grossSalary: number;
    deductions: number;
    netSalary: number;
    status: 'Paid' | 'Pending' | 'Processed';
    processedDate: string;
}

export interface PayrollSummary {
    totalGross: number;
    totalDeductions: number;
    totalNet: number;
    employeeCount: number;
}
