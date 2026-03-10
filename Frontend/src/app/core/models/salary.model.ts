export interface SalaryStructure {
    id: string;
    employeeId: string;
    employeeName: string;
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
    totalDeductions: number;
    netSalary: number;
    ctc: number;
    effectiveDate: string;
    endDate?: string;
    isActive: boolean;
    createdAt: string;
}

export interface SalaryStructureCreate {
    employeeId: string;
    basicSalary: number;
    houseRentAllowance: number;
    transportAllowance: number;
    medicalAllowance: number;
    specialAllowance: number;
    providentFund: number;
    professionalTax: number;
    incomeTax: number;
    otherDeductions: number;
    effectiveDate: string;
}

export interface IncrementRequest {
    id: string;
    employeeId: string;
    employeeName: string;
    currentCTC: number;
    incrementPercentage: number;
    newCTC: number;
    justification: string;
    requestDate: string;
    status: string;
    approvedBy?: string;
    approvedAt?: string;
    remarks?: string;
    createdAt: string;
}

export interface IncrementRequestCreate {
    employeeId: string;
    incrementPercentage: number;
    justification: string;
    requestDate: string;
}

export interface IncrementApproveReject {
    isApproved: boolean;
    remarks?: string;
}
