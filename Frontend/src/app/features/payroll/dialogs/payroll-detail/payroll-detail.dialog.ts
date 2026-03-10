import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { PayrollRecord } from '../../../../core/models/payroll.model';
import { StatusChipComponent, StatusType } from '../../../../shared/components/status-chip/status-chip.component';

@Component({
  selector: 'app-payroll-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, StatusChipComponent],
  template: `
    <div class="p-8 relative max-h-[90vh] overflow-y-auto">
      <button mat-icon-button (click)="onClose()" class="absolute top-4 right-4 text-slate-400 hover:text-slate-600 z-10">
        <mat-icon>close</mat-icon>
      </button>

      <div class="mb-6 flex items-start justify-between">
        <div>
          <h2 class="text-2xl font-black text-slate-900">Salary Statement</h2>
          <p class="text-sm font-bold text-slate-500 uppercase mt-1">{{ getMonthName(record.month) }} {{ record.year }}</p>
        </div>
        <app-status-chip [label]="record.status" [type]="getStatusType(record.status)"></app-status-chip>
      </div>

      <!-- Attendance Info -->
      <div class="bg-slate-50 border border-slate-100 rounded-2xl p-4 mb-6 flex flex-wrap gap-6">
        <div>
          <p class="text-[10px] font-black text-slate-400 uppercase mb-1">Working Days</p>
          <p class="text-lg font-black text-slate-800">{{ record.workingDays }}</p>
        </div>
        <div>
          <p class="text-[10px] font-black text-slate-400 uppercase mb-1">Present</p>
          <p class="text-lg font-black text-green-600">{{ record.presentDays }}</p>
        </div>
        <div>
          <p class="text-[10px] font-black text-slate-400 uppercase mb-1">Absent</p>
          <p class="text-lg font-black text-red-600">{{ record.absentDays }}</p>
        </div>
        <div>
          <p class="text-[10px] font-black text-slate-400 uppercase mb-1">Total Hours</p>
          <p class="text-lg font-black text-slate-800">{{ record.totalWorkHours }}h</p>
        </div>
      </div>

      <!-- Totals -->
      <div class="grid grid-cols-3 gap-4 mb-6">
        <div class="bg-slate-50 p-4 rounded-2xl border border-slate-100">
          <span class="text-[10px] font-black text-slate-400 uppercase mb-1">Gross Salary</span>
          <p class="text-2xl font-black text-slate-700">{{ record.grossSalary | currency }}</p>
        </div>
        <div class="bg-red-50 p-4 rounded-2xl border border-red-100">
          <span class="text-[10px] font-black text-red-500 uppercase mb-1">Total Deductions</span>
          <p class="text-2xl font-black text-red-600">-{{ record.totalDeductions | currency }}</p>
        </div>
        <div class="bg-green-50 p-4 rounded-2xl border border-green-100">
          <span class="text-[10px] font-black text-green-600 uppercase mb-1">Net Salary</span>
          <p class="text-2xl font-black text-green-700">{{ record.netSalary | currency }}</p>
        </div>
      </div>

      <!-- Earnings & Deductions Breakdown -->
      <div class="grid grid-cols-2 gap-8">
        <div>
          <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-4 pb-2 border-b border-slate-100">Earnings</h3>
          <div class="space-y-2">
            <div class="flex justify-between text-sm">
              <span class="text-slate-600">Basic Salary</span>
              <span class="font-bold text-slate-900">{{ record.basicSalary | currency }}</span>
            </div>
            <div class="flex justify-between text-sm" *ngIf="record.houseRentAllowance">
              <span class="text-slate-600">HRA</span>
              <span class="font-bold text-slate-900">{{ record.houseRentAllowance | currency }}</span>
            </div>
            <div class="flex justify-between text-sm" *ngIf="record.transportAllowance">
              <span class="text-slate-600">Transport Allowance</span>
              <span class="font-bold text-slate-900">{{ record.transportAllowance | currency }}</span>
            </div>
            <div class="flex justify-between text-sm" *ngIf="record.medicalAllowance">
              <span class="text-slate-600">Medical Allowance</span>
              <span class="font-bold text-slate-900">{{ record.medicalAllowance | currency }}</span>
            </div>
            <div class="flex justify-between text-sm" *ngIf="record.specialAllowance">
              <span class="text-slate-600">Special Allowance</span>
              <span class="font-bold text-slate-900">{{ record.specialAllowance | currency }}</span>
            </div>
            <div class="flex justify-between text-sm" *ngIf="record.bonusAmount">
              <span class="text-green-600 font-medium">Bonus</span>
              <span class="font-bold text-green-700">+{{ record.bonusAmount | currency }}</span>
            </div>
            <div class="flex justify-between text-sm pt-3 border-t-2 border-dashed border-slate-100 mt-3">
              <span class="font-black text-slate-900 uppercase">Gross Total</span>
              <span class="font-black text-slate-900 text-lg">{{ record.grossSalary | currency }}</span>
            </div>
          </div>
        </div>

        <div>
          <h3 class="text-xs font-black text-red-400 uppercase tracking-widest mb-4 pb-2 border-b border-slate-100">Deductions</h3>
          <div class="space-y-2">
            <div class="flex justify-between text-sm" *ngIf="record.providentFund">
              <span class="text-slate-600">Provident Fund</span>
              <span class="font-bold text-red-500">-{{ record.providentFund | currency }}</span>
            </div>
            <div class="flex justify-between text-sm" *ngIf="record.professionalTax">
              <span class="text-slate-600">Professional Tax</span>
              <span class="font-bold text-red-500">-{{ record.professionalTax | currency }}</span>
            </div>
            <div class="flex justify-between text-sm" *ngIf="record.incomeTax">
              <span class="text-slate-600">Income Tax</span>
              <span class="font-bold text-red-500">-{{ record.incomeTax | currency }}</span>
            </div>
            <div class="flex justify-between text-sm" *ngIf="record.attendanceDeduction">
              <span class="text-slate-600">Attendance Deduction</span>
              <span class="font-bold text-red-500">-{{ record.attendanceDeduction | currency }}</span>
            </div>
            <div class="flex justify-between text-sm" *ngIf="record.unpaidLeaveDeduction">
              <span class="text-slate-600">Unpaid Leave</span>
              <span class="font-bold text-red-500">-{{ record.unpaidLeaveDeduction | currency }}</span>
            </div>
            <div class="flex justify-between text-sm" *ngIf="record.otherDeductions">
              <span class="text-slate-600">Other Deductions</span>
              <span class="font-bold text-red-500">-{{ record.otherDeductions | currency }}</span>
            </div>
            <div class="flex justify-between text-sm pt-3 border-t-2 border-dashed border-slate-100 mt-3">
              <span class="font-black text-red-600 uppercase">Total Deductions</span>
              <span class="font-black text-red-600 text-lg">-{{ record.totalDeductions | currency }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="mt-8 pt-4 border-t border-slate-100 flex justify-end">
        <button mat-flat-button color="primary" class="!rounded-xl !px-8 font-black" (click)="onClose()">Close</button>
      </div>
    </div>
  `
})
export class PayrollDetailDialogComponent {
  private dialogRef = inject(MatDialogRef<PayrollDetailDialogComponent>);
  public data: { record: PayrollRecord } = inject(MAT_DIALOG_DATA);
  get record() { return this.data.record; }

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

  onClose() { this.dialogRef.close(); }
}
