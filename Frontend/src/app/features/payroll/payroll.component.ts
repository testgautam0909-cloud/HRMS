import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PayrollService } from '../../core/services/payroll.service';
import { AuthService } from '../../core/services/auth.service';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusChipComponent, StatusType } from '../../shared/components/status-chip/status-chip.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { PayrollRecord } from '../../core/models/payroll.model';
import { PayrollDetailDialogComponent } from './dialogs/payroll-detail/payroll-detail.dialog';

@Component({
  selector: 'app-payroll',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatDialogModule,
    MatTooltipModule, PageHeaderComponent, StatusChipComponent, EmptyStateComponent
  ],
  templateUrl: './payroll.component.html',
  styleUrl: './payroll.component.css'
})
export class PayrollComponent implements OnInit {
  private payrollService = inject(PayrollService);
  private auth = inject(AuthService);
  private dialog = inject(MatDialog);

  payrolls = signal<PayrollRecord[]>([]);
  lastSalary = signal<PayrollRecord | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);

  ngOnInit() { this.loadPayroll(); }

  loadPayroll() {
    const empId = this.auth.currentUser()?.employeeId;
    if (!empId) return;
    this.isLoading.set(true);
    this.error.set(null);

    this.payrollService.getByEmployee(empId).subscribe({
      next: (res) => {
        const data = res.data || [];
        data.sort((a, b) => a.year !== b.year ? b.year - a.year : b.month - a.month);
        this.payrolls.set(data);
        if (data.length > 0) this.lastSalary.set(data[0]);
        this.isLoading.set(false);
      },
      error: () => { this.error.set('Failed to load payroll history.'); this.isLoading.set(false); }
    });
  }

  downloadSlip(id: string) {
    window.open(this.payrollService.getSalarySlipUrl(id), '_blank');
  }

  viewDetails(record: PayrollRecord) {
    this.dialog.open(PayrollDetailDialogComponent, { width: '700px', data: { record } });
  }

  getMonthName(month: number): string {
    return new Date(2000, month - 1).toLocaleString('default', { month: 'long' });
  }

  getStatusType(status: string): StatusType {
    if (!status) return 'neutral';
    switch (status.toLowerCase()) {
      case 'paid': return 'success';
      case 'generated': return 'warning';
      default: return 'neutral';
    }
  }
}
