export interface EmployeeSummary {
    id: string;
    employeeCode: string;
    fullName: string;
    email: string;
    department: string;
    designation: string;
    isActive: boolean;
    currentShift?: string;
}

export interface Department {
    id: string;
    name: string;
    description?: string;
}

export interface Designation {
    id: string;
    title: string;
}
