import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { PayrollService } from '../../../../core/services/payroll.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-payroll-relock-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="p-6">
      <div class="flex items-center gap-3 mb-6">
        <div class="w-10 h-10 rounded-xl bg-red-50 text-red-600 flex items-center justify-center">
          <mat-icon>lock_open</mat-icon>
        </div>
        <div>
          <h2 class="text-lg font-black text-slate-900">Relock Payroll</h2>
          <p class="text-xs text-slate-400 font-medium">Revert paid status to allow modifications</p>
        </div>
      </div>

      <div class="bg-red-50 border border-red-100 rounded-2xl p-4 mb-6">
        <p class="text-sm text-red-700 font-medium">
          <strong>Warning:</strong> This will relock the payroll record for <strong>{{ data.employeeName }}</strong>, 
          changing its status from Paid back to Generated.
        </p>
      </div>

      <div class="flex justify-end gap-3">
        <button mat-stroked-button (click)="onCancel()" [disabled]="isLoading()" class="!rounded-xl !px-6">Cancel</button>
        <button mat-flat-button color="warn" (click)="onConfirm()" [disabled]="isLoading()" class="!rounded-xl !px-8 font-black">
          <span *ngIf="!isLoading()">Confirm Relock</span>
          <div *ngIf="isLoading()" class="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
        </button>
      </div>
    </div>
  `
})
export class PayrollRelockDialogComponent {
  private dialogRef = inject(MatDialogRef<PayrollRelockDialogComponent>);
  public data: { payrollId: string; employeeName: string } = inject(MAT_DIALOG_DATA);
  private payrollService = inject(PayrollService);
  private toast = inject(ToastService);
  isLoading = signal(false);

  onConfirm() {
    this.isLoading.set(true);
    this.payrollService.relockPayroll(this.data.payrollId).subscribe({
      next: () => { this.toast.success('Payroll relocked'); this.dialogRef.close(true); },
      error: (err) => { this.toast.error(err.error?.message || 'Failed'); this.isLoading.set(false); }
    });
  }

  onCancel() { this.dialogRef.close(false); }
}
