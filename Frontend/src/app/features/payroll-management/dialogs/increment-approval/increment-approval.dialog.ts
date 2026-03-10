import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { SalaryService } from '../../../../core/services/salary.service';
import { ToastService } from '../../../../core/services/toast.service';
import { IncrementRequest } from '../../../../core/models/salary.model';

@Component({
  selector: 'app-increment-approval-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatIconModule],
  template: `
    <div class="p-6">
      <div class="flex items-center gap-3 mb-6">
        <div class="w-10 h-10 rounded-xl bg-orange-50 text-orange-600 flex items-center justify-center">
          <mat-icon>gavel</mat-icon>
        </div>
        <div>
          <h2 class="text-lg font-black text-slate-900">Review Increment Request</h2>
          <p class="text-xs text-slate-400 font-medium">Approve or reject this request</p>
        </div>
      </div>

      <!-- Request Details -->
      <div class="bg-slate-50 rounded-2xl p-4 mb-6 space-y-2">
        <div class="flex justify-between text-sm">
          <span class="text-slate-500 font-medium">Employee</span>
          <span class="font-bold text-slate-900">{{ request.employeeName }}</span>
        </div>
        <div class="flex justify-between text-sm">
          <span class="text-slate-500 font-medium">Current CTC</span>
          <span class="font-bold text-slate-900">{{ request.currentCTC | currency }}</span>
        </div>
        <div class="flex justify-between text-sm">
          <span class="text-slate-500 font-medium">Increment</span>
          <span class="font-black text-green-600">{{ request.incrementPercentage }}%</span>
        </div>
        <div class="flex justify-between text-sm">
          <span class="text-slate-500 font-medium">Proposed CTC</span>
          <span class="font-black text-green-700">{{ request.newCTC | currency }}</span>
        </div>
        <div class="pt-2 border-t border-slate-100">
          <p class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Justification</p>
          <p class="text-sm text-slate-700 font-medium">{{ request.justification }}</p>
        </div>
      </div>

      <form [formGroup]="form" class="space-y-4">
        <mat-form-field appearance="outline" class="w-full">
          <mat-label>Remarks (optional for approval, recommended for rejection)</mat-label>
          <textarea matInput formControlName="remarks" rows="3" placeholder="Add your comments..."></textarea>
        </mat-form-field>

        <div class="flex justify-end gap-3 pt-2">
          <button type="button" mat-stroked-button (click)="onCancel()" [disabled]="isLoading()" class="!rounded-xl !px-6">Cancel</button>
          <button type="button" mat-flat-button color="warn" (click)="onDecision(false)" [disabled]="isLoading()" class="!rounded-xl !px-6 font-black">
            <span *ngIf="!isLoading()">Reject</span>
          </button>
          <button type="button" mat-flat-button color="primary" (click)="onDecision(true)" [disabled]="isLoading()" class="!rounded-xl !px-8 font-black">
            <span *ngIf="!isLoading()">Approve</span>
            <div *ngIf="isLoading()" class="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
          </button>
        </div>
      </form>
    </div>
  `
})
export class IncrementApprovalDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<IncrementApprovalDialogComponent>);
  public data: { request: IncrementRequest } = inject(MAT_DIALOG_DATA);
  private salaryService = inject(SalaryService);
  private toast = inject(ToastService);
  isLoading = signal(false);

  get request() { return this.data.request; }

  form: FormGroup = this.fb.group({ remarks: [''] });

  onDecision(isApproved: boolean) {
    this.isLoading.set(true);
    const dto = { isApproved, remarks: this.form.value.remarks || undefined };
    
    this.salaryService.approveRejectIncrement(this.request.id, dto).subscribe({
      next: () => {
        this.toast.success(isApproved ? 'Increment approved' : 'Increment rejected');
        this.dialogRef.close(true);
      },
      error: (err) => { this.toast.error(err.error?.message || 'Failed'); this.isLoading.set(false); }
    });
  }

  onCancel() { this.dialogRef.close(false); }
}
