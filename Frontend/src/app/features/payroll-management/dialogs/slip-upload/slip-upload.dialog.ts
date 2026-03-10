import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { PayrollService } from '../../../../core/services/payroll.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-slip-upload-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="p-6">
      <div class="flex items-center gap-3 mb-6">
        <div class="w-10 h-10 rounded-xl bg-primary-50 text-primary-600 flex items-center justify-center">
          <mat-icon>cloud_upload</mat-icon>
        </div>
        <div>
          <h2 class="text-lg font-black text-slate-900">Upload Salary Slip</h2>
          <p class="text-xs text-slate-400 font-medium">Generate and upload PDF to cloud for {{ data.employeeName }}</p>
        </div>
      </div>

      <div class="bg-slate-50 border border-slate-100 rounded-2xl p-4 mb-6">
        <p class="text-sm text-slate-600 font-medium">
          This will generate a PDF salary slip on the server and upload it to cloud storage (Cloudinary).
          The generated URL will be available for download.
        </p>
      </div>

      <div *ngIf="uploadedUrl()" class="bg-green-50 border border-green-100 rounded-2xl p-4 mb-6">
        <p class="text-xs font-black text-green-600 uppercase tracking-widest mb-1">Upload Successful</p>
        <a [href]="uploadedUrl()" target="_blank" class="text-sm text-primary-600 font-bold hover:underline break-all">{{ uploadedUrl() }}</a>
      </div>

      <div class="flex justify-end gap-3">
        <button mat-stroked-button (click)="onCancel()" class="!rounded-xl !px-6">{{ uploadedUrl() ? 'Close' : 'Cancel' }}</button>
        <button *ngIf="!uploadedUrl()" mat-flat-button color="primary" (click)="onUpload()" [disabled]="isLoading()" class="!rounded-xl !px-8 font-black">
          <span *ngIf="!isLoading()">Generate & Upload</span>
          <div *ngIf="isLoading()" class="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
        </button>
      </div>
    </div>
  `
})
export class SlipUploadDialogComponent {
  private dialogRef = inject(MatDialogRef<SlipUploadDialogComponent>);
  public data: { payrollId: string; employeeName: string } = inject(MAT_DIALOG_DATA);
  private payrollService = inject(PayrollService);
  private toast = inject(ToastService);
  isLoading = signal(false);
  uploadedUrl = signal<string | null>(null);

  onUpload() {
    this.isLoading.set(true);
    this.payrollService.uploadSalarySlip(this.data.payrollId).subscribe({
      next: (res) => {
        this.uploadedUrl.set(res.data?.Url || res.data?.url || 'Uploaded');
        this.toast.success('Salary slip uploaded to cloud');
        this.isLoading.set(false);
      },
      error: (err) => { this.toast.error(err.error?.message || 'Upload failed'); this.isLoading.set(false); }
    });
  }

  onCancel() { this.dialogRef.close(this.uploadedUrl() ? true : false); }
}
