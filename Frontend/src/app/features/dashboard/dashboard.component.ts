import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgChartsModule } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { ApiResponse } from '../../core/models/api-response.model';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import { RouterModule } from '@angular/router';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { StatusChipComponent } from '../../shared/components/status-chip/status-chip.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatTooltipModule,
    RouterModule,
    LoadingSkeletonComponent,
    EmptyStateComponent,
    NgChartsModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private api = inject(ApiService);
  public auth = inject(AuthService);

  today = new Date();
  isLoading = signal(true);

  // Stats
  totalEmployees = signal(0);
  activeEmployees = signal(0);
  onLeaveToday = signal(0);
  avgWorkHours = signal('0');
  attendanceRate = signal(0);
  payrollStatus = signal('Pending');
  pendingLeavesCount = signal(0);

  // Lists
  recentEmployees = signal<any[]>([]);
  pendingLeaves = signal<any[]>([]);

  // Charts
  public barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: '#1e293b',
        titleFont: { size: 12, weight: 'bold' },
        bodyFont: { size: 11 },
        cornerRadius: 8,
        padding: 10,
        callbacks: {
          label: (context: any) => `Working Hours: ${context.raw}h`
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        grid: { color: '#f1f5f9' },
        ticks: { font: { size: 10, weight: 'bold' }, color: '#94a3b8' }
      },
      x: {
        grid: { display: false },
        ticks: { font: { size: 10, weight: 'bold' }, color: '#94a3b8' }
      }
    }
  };

  public barChartData: ChartConfiguration['data'] = {
    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
    datasets: [{
      label: 'Hours Worked',
      data: [8, 7.5, 9, 8.5, 8, 4],
      backgroundColor: '#3167d1',
      hoverBackgroundColor: '#2c5cbc',
      borderRadius: 8,
    }]
  };

  public pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          padding: 16,
          usePointStyle: true,
          font: { size: 11, weight: 'bold' },
          color: '#64748b'
        }
      },
      tooltip: {
        backgroundColor: '#1e293b',
        cornerRadius: 8,
        padding: 10,
      }
    }
  };

  public pieChartData: ChartConfiguration['data'] = {
    labels: ['Sick Leave', 'Annual Leave', 'Casual Leave', 'Other'],
    datasets: [{
      data: [15, 45, 10, 30],
      backgroundColor: ['#f6ae40', '#3167d1', '#28a745', '#17a2b8'],
      hoverOffset: 10,
      borderWidth: 2,
      borderColor: '#ffffff'
    }]
  };

  ngOnInit() {
    this.loadAllData();
  }

  loadAllData() {
    this.isLoading.set(true);
    const user = this.auth.currentUser();
    const empId = user?.employeeId;

    // Load Employees
    this.api.get<any>('employee', { pageNumber: 1, pageSize: 5 })
      .pipe(catchError(() => of({ success: false, data: { data: [], pagination: { totalCount: 0 } } } as any)))
      .subscribe(res => {
        const data = res.data?.data || [];
        this.recentEmployees.set(data.map((e: any) => ({
          id: e.id,
          initials: (e.firstName?.[0] || '') + (e.lastName?.[0] || ''),
          name: (e.firstName || '') + ' ' + (e.lastName || ''),
          designation: e.designationName || 'N/A',
          dept: e.departmentName || 'N/A',
          joinedDate: e.joiningDate ? new Date(e.joiningDate) : null,
          profilePicture: e.profilePictureUrl
        })));
        this.totalEmployees.set(res.data?.pagination?.totalCount || data.length || 0);
        this.activeEmployees.set(res.data?.pagination?.totalCount || data.length || 0);
      });

    // Load Pending Leaves
    this.api.get<any>('leave', { status: 1, pageSize: 5 })
      .pipe(catchError(() => of({ success: false, data: { data: [], pagination: { totalCount: 0 } } } as any)))
      .subscribe(res => {
        const data = res.data?.data || [];
        this.pendingLeaves.set(data.map((l: any) => ({
          id: l.id,
          name: l.employeeName || 'Unknown',
          type: l.leaveTypeName || 'Leave',
          days: l.days || 0,
          date: l.startDate ? new Date(l.startDate) : new Date()
        })));
        this.pendingLeavesCount.set(res.data?.pagination?.totalCount || data.length || 0);
        this.onLeaveToday.set(res.data?.pagination?.totalCount || 0);
      });

    // Load Attendance Summary
    if (empId && empId !== 'null' && empId !== 'undefined') {
      this.api.get<any>(`attendance/summary/${empId}`, {
        month: this.today.getMonth() + 1,
        year: this.today.getFullYear()
      })
        .pipe(catchError(() => of({ data: null })))
        .subscribe(res => {
          if (res.data) {
            const avg = res.data.averageWorkingHours?.toFixed(1) || '0';
            this.avgWorkHours.set(avg);
            const totalPresent = res.data.totalPresent || 0;
            const totalDays = res.data.totalWorkingDays || 1;
            this.attendanceRate.set(Math.round((totalPresent / totalDays) * 100));
          }
        });

      // Load Leave Balances for Pie Chart
      this.api.get<any[]>(`leave/balance/${empId}`, { year: this.today.getFullYear() })
        .pipe(catchError(() => of({ data: [] })))
        .subscribe(res => {
          if (res.data && Array.isArray(res.data) && res.data.length > 0) {
            const balances = res.data;
            this.pieChartData = {
              labels: balances.map((b: any) => b.leaveTypeName || 'Leave'),
              datasets: [{
                data: balances.map((b: any) => b.remainingDays || 0),
                backgroundColor: ['#f6ae40', '#3167d1', '#28a745', '#dc3545', '#17a2b8', '#6366f1'],
                hoverOffset: 10,
                borderWidth: 2,
                borderColor: '#ffffff'
              }]
            };
          }
        });
    }

    // Load Payroll Status
    const isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('HR');
    const payrollObs = isAdmin
      ? this.api.get<any>('payroll/summary', { month: this.today.getMonth() + 1, year: this.today.getFullYear() })
      : (empId && empId !== 'null')
        ? this.api.get<any>(`payroll/employee/${empId}`, { month: this.today.getMonth() + 1, year: this.today.getFullYear() })
        : of({ success: false, data: null } as any);

    payrollObs
      .pipe(catchError(() => of({ success: false, data: null } as any)))
      .subscribe(res => {
        if (isAdmin) {
          this.payrollStatus.set(res.data?.totalAmount ? 'Processed' : 'Pending');
        } else {
          this.payrollStatus.set(res.data ? 'Paid' : 'Pending');
        }
      });

    // Set loading to false after a short delay to ensure all calls start
    setTimeout(() => this.isLoading.set(false), 1200);
  }
}
