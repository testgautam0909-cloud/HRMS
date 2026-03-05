import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgChartsModule } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { ApiResponse } from '../../core/models/api-response.model';
import { catchError, finalize, of } from 'rxjs';

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
    RouterModule,
    LoadingSkeletonComponent,
    EmptyStateComponent,
    StatusChipComponent,
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

  // Per-widget loading states
  statsLoading = signal(true);
  employeesLoading = signal(true);
  leavesLoading = signal(true);
  payrollLoading = signal(true);
  attendanceLoading = signal(true);

  // Data signals
  stats = signal<any[]>([]);
  recentEmployees = signal<any[]>([]);
  upcomingLeaves = signal<any[]>([]);

  public barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (context) => `Working Hours: ${context.raw}h`
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        grid: { display: false },
        title: { display: true, text: 'Hours', font: { size: 10, weight: 'bold' } }
      },
      x: { grid: { display: false } }
    }
  };

  public barChartData: ChartConfiguration['data'] = {
    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
    datasets: [{
      label: 'Hours Worked',
      data: [8, 7.5, 9, 8.5, 8, 4],
      backgroundColor: '#6366f1',
      hoverBackgroundColor: '#4f46e5',
      borderRadius: 10,
    }]
  };

  public pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          padding: 20,
          usePointStyle: true,
          font: { size: 11, weight: 'bold' }
        }
      }
    }
  };

  public pieChartData: ChartConfiguration['data'] = {
    labels: ['Sick', 'Annual', 'Paternity', 'Other'],
    datasets: [{
      data: [15, 45, 10, 30],
      backgroundColor: ['#8b5cf6', '#0ea5e9', '#10b981', '#f59e0b'],
      hoverOffset: 15,
      borderWidth: 0
    }]
  };

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    const user = this.auth.currentUser();
    const empId = user?.employeeId;
    this.isLoading.set(false);

    // Load Stats & Summary (Attendance)
    if (empId && empId !== 'null' && empId !== 'undefined') {
      this.attendanceLoading.set(true);
      this.api.get<any>(`attendance/summary/${empId}`, { month: this.today.getMonth() + 1, year: this.today.getFullYear() })
        .pipe(catchError(() => of({ data: null })), finalize(() => this.attendanceLoading.set(false)))
        .subscribe(res => {
          const avgHours = res.data?.averageWorkingHours?.toFixed(1) || '0';
          this.updateStats('Avg Working Hours', avgHours + 'h', 'schedule', 'bg-green-50 text-green-600');
        });
    } else {
      this.attendanceLoading.set(false);
    }

    // Load Employees
    this.employeesLoading.set(true);
    this.api.get<any>('employee', { pageNumber: 1, pageSize: 5 })
      .pipe(
        catchError(() => of({ success: false, data: { data: [], pagination: { totalCount: 0 } }, message: '', errors: [] } as ApiResponse<any>)),
        finalize(() => this.employeesLoading.set(false))
      )
      .subscribe(res => {
        const data = res.data?.data || [];
        this.recentEmployees.set(data.map((e: any) => ({
          id: e.id,
          initials: e.firstName[0] + e.lastName[0],
          name: e.firstName + ' ' + e.lastName,
          designation: e.designationName,
          dept: e.departmentName,
          joinedDate: new Date(e.joiningDate)
        })));
        this.updateStats('Total Employees', res.data?.pagination?.totalCount || 0, 'group', 'bg-primary-50 text-primary-600');
      });

    // Load Payroll
    this.payrollLoading.set(true);
    const payrollObs = this.auth.hasRole('Admin') || this.auth.hasRole('HR')
      ? this.api.get<any>('payroll/summary', { month: this.today.getMonth() + 1, year: this.today.getFullYear() })
      : this.api.get<any>(`payroll/employee/${empId}`, { month: this.today.getMonth() + 1, year: this.today.getFullYear() });

    payrollObs.pipe(
      catchError(() => of({ success: false, data: null, message: '', errors: [] } as ApiResponse<any>)),
      finalize(() => this.payrollLoading.set(false))
    )
      .subscribe(res => {
        const val = this.auth.hasRole('Admin') || this.auth.hasRole('HR')
          ? (res.data?.totalAmount ? 'Processed' : 'Pending')
          : (res.data ? 'Paid' : 'Pending');
        this.updateStats('Payroll Status', val, 'verified', 'bg-blue-50 text-blue-600');
      });

    // Load Leaves & Balances
    this.leavesLoading.set(true);
    this.api.get<any>('leave', { status: 1, pageSize: 5 })
      .pipe(
        catchError(() => of({ success: false, data: { data: [], pagination: { totalCount: 0 } }, message: '', errors: [] } as ApiResponse<any>)),
        finalize(() => this.leavesLoading.set(false))
      )
      .subscribe(res => {
        const data = res.data?.data || [];
        this.upcomingLeaves.set(data.map((l: any) => ({
          name: l.employeeName,
          type: l.leaveTypeName,
          days: l.days,
          date: new Date(l.startDate)
        })));
        this.updateStats('On Leave Today', res.data?.pagination?.totalCount || 0, 'event', 'bg-accent-50 text-accent-700');
      });

    if (empId && empId !== 'null' && empId !== 'undefined') {
      this.api.get<any[]>(`leave/balance/${empId}`, { year: this.today.getFullYear() })
        .pipe(catchError(() => of({ data: [] })))
        .subscribe(res => {
          if (res.data) {
            const balances = res.data;
            this.pieChartData = {
              labels: balances.map((b: any) => b.leaveTypeName),
              datasets: [{
                data: balances.map((b: any) => b.remainingDays),
                backgroundColor: ['#8b5cf6', '#0ea5e9', '#10b981', '#f59e0b', '#ec4899'],
                borderWidth: 0
              }]
            };
          }
        });
    }
  }

  private updateStats(label: string, value: any, icon: string, colorClass: string) {
    const current = this.stats();
    const index = current.findIndex(s => s.label === label);
    const newItem = { label, value, trend: 0, icon, colorClass };

    if (index >= 0) {
      current[index] = newItem;
      this.stats.set([...current]);
    } else {
      this.stats.set([...current, newItem]);
    }
  }
}
