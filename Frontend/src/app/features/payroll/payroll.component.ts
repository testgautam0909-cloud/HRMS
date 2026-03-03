import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusChipComponent, StatusType } from '../../shared/components/status-chip/status-chip.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { PayrollRecord } from '../../core/models/payroll.model';

@Component({
  selector: 'app-payroll',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    PageHeaderComponent,
    StatusChipComponent,
    EmptyStateComponent
  ],
  templateUrl: './payroll.component.html',
  styleUrl: './payroll.component.css'
})
export class PayrollComponent implements OnInit {
  private api = inject(ApiService);
  private auth = inject(AuthService);

  payrolls = signal<any[]>([]);
  lastSalary = signal<any>(null);

  ngOnInit() {
    this.loadPayroll();
  }

  loadPayroll() {
    const empId = this.auth.currentUser()?.employeeId;
    if (!empId) return;

    this.api.get<any>('payroll', { employeeId: empId }).subscribe({
      next: (res) => {
        const data = res.data.map((r: any) => ({
          ...r,
          monthName: new Date(2024, r.month - 1).toLocaleString('default', { month: 'long' })
        }));
        this.payrolls.set(data);
        if (data.length > 0) {
          this.lastSalary.set(data[0]);
        }
      }
    });
  }

  downloadSlip(id: string) {
    window.open(`${this.api['baseUrl']}/payroll/${id}/salary-slip`, '_blank');
  }

  getStatusType(status: string): StatusType {
    switch (status.toLowerCase()) {
      case 'paid': return 'success';
      case 'pending': return 'warning';
      case 'rejected': return 'error';
      default: return 'neutral';
    }
  }
}
