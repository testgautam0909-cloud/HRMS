import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { PayrollService } from '../../../../core/services/payroll.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-mark-paid-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="p-6">
      <div class="flex items-center gap-3 mb-6">
        <div class="w-10 h-10 rounded-xl bg-green-50 text-green-600 flex items-center justify-center">
          <mat-icon>check_circle</mat-icon>
        </div>
        <div>
          <h2 class="text-lg font-black text-slate-900">Mark as Paid</h2>
          <p class="text-xs text-slate-400 font-medium">Confirm payment for this record</p>
        </div>
      </div>

      <div class="bg-green-50 border border-green-100 rounded-2xl p-4 mb-6">
        <p class="text-sm text-slate-700 font-medium">
          You are about to mark payroll for <strong>{{ data.employeeName }}</strong> as paid.
        </p>
        <p class="text-2xl font-black text-green-700 mt-2">{{ data.netPay | currency }}</p>
      </div>

      <div class="flex justify-end gap-3">
        <button mat-stroked-button (click)="onCancel()" [disabled]="isLoading()" class="!rounded-xl !px-6">Cancel</button>
        <button mat-flat-button color="primary" (click)="onConfirm()" [disabled]="isLoading()" class="!rounded-xl !px-8 font-black">
          <span *ngIf="!isLoading()">Confirm Payment</span>
          <div *ngIf="isLoading()" class="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
        </button>
      </div>
    </div>
  `
})
export class MarkPaidDialogComponent {
  private dialogRef = inject(MatDialogRef<MarkPaidDialogComponent>);
  public data: { payrollId: string; employeeName: string; netPay: number } = inject(MAT_DIALOG_DATA);
  private payrollService = inject(PayrollService);
  private toast = inject(ToastService);
  isLoading = signal(false);

  onConfirm() {
    this.isLoading.set(true);
    this.payrollService.markAsPaid(this.data.payrollId).subscribe({
      next: () => { this.toast.success('Marked as paid'); this.dialogRef.close(true); },
      error: (err) => { this.toast.error(err.error?.message || 'Failed'); this.isLoading.set(false); }
    });
  }

  onCancel() { this.dialogRef.close(false); }
}
