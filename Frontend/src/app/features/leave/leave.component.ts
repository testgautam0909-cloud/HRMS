import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusChipComponent, StatusType } from '../../shared/components/status-chip/status-chip.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-leave',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatProgressBarModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    PageHeaderComponent,
    StatusChipComponent,
    EmptyStateComponent
  ],
  templateUrl: './leave.component.html',
  styleUrl: './leave.component.css'
})
export class LeaveComponent implements OnInit {
  private api = inject(ApiService);
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);

  balances = signal<any[]>([]);
  leaves = signal<any[]>([]);
  leaveTypes = signal<any[]>([]);
  showApplyModal = signal(false);
  isLoading = signal(false);

  leaveForm = this.fb.group({
    leaveTypeId: ['', Validators.required],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    reason: ['', Validators.required]
  });

  ngOnInit() {
    this.loadBalances();
    this.loadHistory();
    this.loadLeaveTypes();
  }

  loadBalances() {
    const user = this.auth.currentUser();
    const empId = user?.employeeId;
    if (!empId) return;
    this.api.get<any[]>(`leave/balance/${empId}`, { year: new Date().getFullYear() }).subscribe({
      next: (res: any) => this.balances.set(res.data)
    });
  }

  loadHistory() {
    const user = this.auth.currentUser();
    const empId = user?.employeeId;
    this.api.get<any>('leave', { employeeId: empId }).subscribe({
      next: (res: any) => this.leaves.set(res.data)
    });
  }

  loadLeaveTypes() {
    this.api.get<any[]>('leave/types').subscribe({
      next: (res: any) => this.leaveTypes.set(res.data)
    });
  }

  calculateDays(): number {
    const start = this.leaveForm.value.startDate;
    const end = this.leaveForm.value.endDate;
    if (!start || !end) return 0;

    const diffTime = Math.abs(new Date(end).getTime() - new Date(start).getTime());
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
  }

  onApply() {
    if (this.leaveForm.invalid) return;
    this.isLoading.set(true);
    this.api.post<any>('leave/apply', this.leaveForm.value).subscribe({
      next: () => {
        this.showApplyModal.set(false);
        this.loadHistory();
        this.loadBalances();
        this.isLoading.set(false);
        this.leaveForm.reset();
      },
      error: () => this.isLoading.set(false)
    });
  }

  getStatusType(status: string): StatusType {
    switch (status.toLowerCase()) {
      case 'approved': return 'success';
      case 'pending': return 'warning';
      case 'rejected': return 'error';
      default: return 'neutral';
    }
  }
}
