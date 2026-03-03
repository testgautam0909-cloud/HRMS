export interface Shift {
    id: string;
    name: string;
    startTime: string;
    endTime: string;
}

export interface ShiftSchedule {
    id: string;
    employeeId: string;
    date: string;
    shiftId: string;
    shiftName: string;
    startTime: string;
    endTime: string;
    isToday?: boolean;
}

export interface EmployeeShiftSummary {
    currentShift: ShiftSchedule | null;
    upcomingSchedule: ShiftSchedule[];
}
