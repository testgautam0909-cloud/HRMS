import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';

import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusChipComponent, StatusType } from '../../shared/components/status-chip/status-chip.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

import { PayrollService } from '../../core/services/payroll.service';
import { SalaryService } from '../../core/services/salary.service';
import { PayrollRecord, PayrollSummary } from '../../core/models/payroll.model';
import { IncrementRequest } from '../../core/models/salary.model';

import { PayrollGenerationDialogComponent } from './dialogs/payroll-generation/payroll-generation.dialog';
import { IncrementApprovalDialogComponent } from './dialogs/increment-approval/increment-approval.dialog';
import { MarkPaidDialogComponent } from './dialogs/mark-paid/mark-paid.dialog';
import { PayrollOverrideDialogComponent } from './dialogs/payroll-override/payroll-override.dialog';
import { PayrollRelockDialogComponent } from './dialogs/payroll-relock/payroll-relock.dialog';
import { SlipUploadDialogComponent } from './dialogs/slip-upload/slip-upload.dialog';

@Component({
  selector: 'app-payroll-management',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule, MatIconModule,
    MatPaginatorModule, MatSelectModule, MatDialogModule, MatTooltipModule, MatMenuModule,
    MatDividerModule, PageHeaderComponent, StatusChipComponent, EmptyStateComponent,
    CurrencyPipe, DatePipe
  ],
  templateUrl: './payroll-management.component.html',
  styleUrl: './payroll-management.component.css'
})
export class PayrollManagementComponent implements OnInit {
  private payrollService = inject(PayrollService);
  private salaryService = inject(SalaryService);
  private dialog = inject(MatDialog);

  months = [
    { value: 1, label: 'January' }, { value: 2, label: 'February' },
    { value: 3, label: 'March' }, { value: 4, label: 'April' },
    { value: 5, label: 'May' }, { value: 6, label: 'June' },
    { value: 7, label: 'July' }, { value: 8, label: 'August' },
    { value: 9, label: 'September' }, { value: 10, label: 'October' },
    { value: 11, label: 'November' }, { value: 12, label: 'December' }
  ];
  years = Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - i + 1);

  selectedMonth = new FormControl(new Date().getMonth() + 1);
  selectedYear = new FormControl(new Date().getFullYear());
  statusFilter = new FormControl('');

  summary = signal<PayrollSummary | null>(null);
  payrolls = signal<PayrollRecord[]>([]);
  totalRecords = signal(0);
  pageSize = signal(10);
  pageIndex = signal(0);
  pendingIncrements = signal<IncrementRequest[]>([]);
  showIncrements = signal<boolean>(true);
  isLoading = signal(false);

  completionPercent = computed(() => {
    const s = this.summary();
    if (!s || !s.totalEmployees) return 0;
    return Math.round((s.totalPaid / s.totalEmployees) * 100);
  });

  ngOnInit() {
    this.loadData();
    this.selectedMonth.valueChanges.subscribe(() => this.loadData());
    this.selectedYear.valueChanges.subscribe(() => this.loadData());
    this.statusFilter.valueChanges.subscribe(() => this.loadPayrollRecords());
  }

  loadData() {
    this.loadSummary();
    this.loadPayrollRecords();
    this.loadPendingIncrements();
  }

  loadSummary() {
    const m = this.selectedMonth.value || new Date().getMonth() + 1;
    const y = this.selectedYear.value || new Date().getFullYear();
    this.payrollService.getSummary(m, y).subscribe({
      next: (res) => this.summary.set(res.data),
      error: () => this.summary.set(null)
    });
  }

  loadPayrollRecords() {
    this.isLoading.set(true);
    const params: any = {
      month: this.selectedMonth.value || undefined,
      year: this.selectedYear.value || undefined,
      page: this.pageIndex() + 1,
      pageSize: this.pageSize()
    };

    this.payrollService.getAll(params).subscribe({
      next: (res: any) => {
        if (Array.isArray(res.data)) {
          this.payrolls.set(res.data);
          this.totalRecords.set(res.pagination?.totalCount || res.data.length);
        } else if (res.items) {
          this.payrolls.set(res.items);
          this.totalRecords.set(res.totalCount || 0);
        } else {
          this.payrolls.set([]);
          this.totalRecords.set(0);
        }
        this.isLoading.set(false);
      },
      error: () => { this.payrolls.set([]); this.isLoading.set(false); }
    });
  }

  loadPendingIncrements() {
    this.salaryService.getIncrements(undefined, 1, 50).subscribe({
      next: (res: any) => {
        const items = res.items || (Array.isArray(res.data) ? res.data : []);
        // Filter pending on frontend since backend doesn't support status filter
        this.pendingIncrements.set(items.filter((i: any) => i.status === 'Pending'));
      },
      error: () => this.pendingIncrements.set([])
    });
  }

  onPageChange(event: PageEvent) {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.loadPayrollRecords();
  }

  openGenerateDialog() {
    const ref = this.dialog.open(PayrollGenerationDialogComponent, { width: '560px' });
    ref.afterClosed().subscribe(res => { if (res) this.loadData(); });
  }

  openMarkPaidDialog(record: PayrollRecord) {
    const ref = this.dialog.open(MarkPaidDialogComponent, {
      width: '450px',
      data: { payrollId: record.id, employeeName: record.employeeName, netPay: record.netSalary }
    });
    ref.afterClosed().subscribe(res => { if (res) this.loadData(); });
  }

  openOverrideDialog(record: PayrollRecord) {
    const ref = this.dialog.open(PayrollOverrideDialogComponent, {
      width: '500px', data: { payrollId: record.id, employeeName: record.employeeName }
    });
    ref.afterClosed().subscribe(res => { if (res) this.loadData(); });
  }

  openRelockDialog(record: PayrollRecord) {
    const ref = this.dialog.open(PayrollRelockDialogComponent, {
      width: '450px', data: { payrollId: record.id, employeeName: record.employeeName }
    });
    ref.afterClosed().subscribe(res => { if (res) this.loadData(); });
  }

  openSlipUploadDialog(record: PayrollRecord) {
    const ref = this.dialog.open(SlipUploadDialogComponent, {
      width: '450px', data: { payrollId: record.id, employeeName: record.employeeName }
    });
    ref.afterClosed().subscribe(() => {});
  }

  openIncrementApproval(request: IncrementRequest) {
    const ref = this.dialog.open(IncrementApprovalDialogComponent, {
      width: '500px', data: { request }
    });
    ref.afterClosed().subscribe(res => { if (res) this.loadPendingIncrements(); });
  }

  downloadSlip(id: string) {
    const slipUrl = this.payrollService.getSalarySlipUrl(id);
    window.open(slipUrl, '_blank');
  }

  getMonthName(month: number): string {
    return new Date(2000, month - 1).toLocaleString('default', { month: 'long' });
  }

  getStatusType(status: string): StatusType {
    if (!status) return 'neutral';
    switch (status.toLowerCase()) {
      case 'paid': return 'success';
      case 'generated': return 'warning';
      case 'overridden': return 'info';
      case 'relocked': return 'error';
      default: return 'neutral';
    }
  }
}
