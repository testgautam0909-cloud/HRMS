export interface DashboardSummary {
    totalEmployees: number;
    activeEmployees: number;
    onLeaveTodayCount: number;
    onLeaveTodayAvatars: EmployeeAvatar[];
    attendanceRateToday: number;
    pendingLeavesCount: number;
    payrollProcessedThisMonth: boolean;
    avgWorkingHoursMonth: number;
    attendanceTrend: ChartPoint[];
    leaveDistribution: ChartPoint[];
    recentActivities: RecentActivity[];
}

export interface EmployeeAvatar {
    name: string;
    avatarUrl?: string;
    initials: string;
}

export interface ChartPoint {
    label: string;
    value: number;
}

export interface RecentActivity {
    description: string;
    timestamp: string;
    type: 'Leave' | 'Attendance' | 'Payroll';
}
