export interface AttendanceRecord {
    id: string;
    employeeId: string;
    employeeName: string;
    date: string;
    checkInTime: string;
    checkOutTime?: string;
    workHours?: number;
    status: 'Present' | 'Late' | 'Absent' | 'OnLeave';
    correctionNote?: string;
}

export interface AttendanceSummary {
    employeeId: string;
    employeeName: string;
    month: number;
    year: number;
    totalWorkingDays: number;
    daysPresent: number;
    daysAbsent: number;
    halfDays: number;
    totalWorkHours: number;
    averageDailyHours: number;
    records: AttendanceRecord[];
}
