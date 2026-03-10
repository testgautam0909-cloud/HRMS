import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { SalaryService } from '../../../../core/services/salary.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-salary-structure-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule, MatButtonModule,
    MatFormFieldModule, MatInputModule, MatDatepickerModule, MatNativeDateModule, MatIconModule
  ],
  templateUrl: './salary-structure.dialog.html',
  styleUrl: './salary-structure.dialog.css'
})
export class SalaryStructureDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<SalaryStructureDialogComponent>);
  public data: { employeeId: string; salary?: any } = inject(MAT_DIALOG_DATA);
  private salaryService = inject(SalaryService);
  private toast = inject(ToastService);

  form!: FormGroup;
  isLoading = signal(false);

  ngOnInit() {
    const s = this.data.salary;
    this.form = this.fb.group({
      basicSalary: [s?.basicSalary || 0, [Validators.required, Validators.min(1)]],
      houseRentAllowance: [s?.houseRentAllowance || 0, Validators.min(0)],
      transportAllowance: [s?.transportAllowance || 0, Validators.min(0)],
      medicalAllowance: [s?.medicalAllowance || 0, Validators.min(0)],
      specialAllowance: [s?.specialAllowance || 0, Validators.min(0)],
      providentFund: [s?.providentFund || 0, Validators.min(0)],
      professionalTax: [s?.professionalTax || 0, Validators.min(0)],
      incomeTax: [s?.incomeTax || 0, Validators.min(0)],
      otherDeductions: [s?.otherDeductions || 0, Validators.min(0)],
      effectiveDate: [s?.effectiveDate ? new Date(s.effectiveDate) : new Date(), Validators.required]
    });
  }

  get grossSalary(): number {
    const v = this.form?.value;
    if (!v) return 0;
    return (v.basicSalary || 0) + (v.houseRentAllowance || 0) + (v.transportAllowance || 0) +
           (v.medicalAllowance || 0) + (v.specialAllowance || 0);
  }

  get totalDeductions(): number {
    const v = this.form?.value;
    if (!v) return 0;
    return (v.providentFund || 0) + (v.professionalTax || 0) + (v.incomeTax || 0) + (v.otherDeductions || 0);
  }

  get netSalary(): number {
    return this.grossSalary - this.totalDeductions;
  }

  onSubmit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isLoading.set(true);

    const payload = {
      employeeId: this.data.employeeId,
      ...this.form.value,
      effectiveDate: this.form.value.effectiveDate instanceof Date
        ? this.form.value.effectiveDate.toISOString()
        : this.form.value.effectiveDate
    };

    this.salaryService.createStructure(payload).subscribe({
      next: () => { this.toast.success('Salary structure updated'); this.dialogRef.close(true); },
      error: (err) => { this.toast.error(err.error?.message || 'Failed'); this.isLoading.set(false); }
    });
  }

  onCancel() { this.dialogRef.close(false); }
}
