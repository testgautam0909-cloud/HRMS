import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { PayrollService } from '../../../../core/services/payroll.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-payroll-override-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatIconModule],
  template: `
    <div class="p-6">
      <div class="flex items-center gap-3 mb-6">
        <div class="w-10 h-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center">
          <mat-icon>edit_note</mat-icon>
        </div>
        <div>
          <h2 class="text-lg font-black text-slate-900">Override Payroll</h2>
          <p class="text-xs text-slate-400 font-medium">Adjust bonus/penalty for {{ data.employeeName }}</p>
        </div>
      </div>

      <form [formGroup]="form" (ngSubmit)="onSubmit()" class="space-y-4">
        <mat-form-field appearance="outline" class="w-full">
          <mat-label>Bonus Amount</mat-label>
          <input matInput type="number" formControlName="bonusAmount" min="0">
          <mat-icon matPrefix class="text-green-500 mr-2">add_circle</mat-icon>
          <mat-hint>Leave 0 or empty if no bonus</mat-hint>
        </mat-form-field>

        <mat-form-field appearance="outline" class="w-full">
          <mat-label>Penalty Amount</mat-label>
          <input matInput type="number" formControlName="penaltyAmount" min="0">
          <mat-icon matPrefix class="text-red-500 mr-2">remove_circle</mat-icon>
          <mat-hint>Leave 0 or empty if no penalty</mat-hint>
        </mat-form-field>

        <mat-form-field appearance="outline" class="w-full">
          <mat-label>Reason for Override</mat-label>
          <textarea matInput formControlName="overrideReason" rows="3" placeholder="Explain why this override is needed..."></textarea>
        </mat-form-field>

        <div class="flex justify-end gap-3 pt-2">
          <button type="button" mat-stroked-button (click)="onCancel()" [disabled]="isLoading()" class="!rounded-xl !px-6">Cancel</button>
          <button type="submit" mat-flat-button color="primary" [disabled]="form.invalid || isLoading()" class="!rounded-xl !px-8 font-black">
            <span *ngIf="!isLoading()">Apply Override</span>
            <div *ngIf="isLoading()" class="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
          </button>
        </div>
      </form>
    </div>
  `
})
export class PayrollOverrideDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<PayrollOverrideDialogComponent>);
  public data: { payrollId: string; employeeName: string } = inject(MAT_DIALOG_DATA);
  private payrollService = inject(PayrollService);
  private toast = inject(ToastService);
  isLoading = signal(false);

  form: FormGroup = this.fb.group({
    bonusAmount: [0],
    penaltyAmount: [0],
    overrideReason: ['', Validators.required]
  });

  onSubmit() {
    if (this.form.invalid) return;
    this.isLoading.set(true);

    const dto = {
      bonusAmount: this.form.value.bonusAmount || undefined,
      penaltyAmount: this.form.value.penaltyAmount || undefined,
      overrideReason: this.form.value.overrideReason
    };

    this.payrollService.overridePayroll(this.data.payrollId, dto).subscribe({
      next: () => { this.toast.success('Payroll overridden'); this.dialogRef.close(true); },
      error: (err) => { this.toast.error(err.error?.message || 'Failed'); this.isLoading.set(false); }
    });
  }

  onCancel() { this.dialogRef.close(false); }
}
