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
  selector: 'app-increment-request-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule, MatButtonModule,
    MatFormFieldModule, MatInputModule, MatDatepickerModule, MatNativeDateModule, MatIconModule
  ],
  templateUrl: './increment-request.dialog.html',
  styleUrl: './increment-request.dialog.css'
})
export class IncrementRequestDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<IncrementRequestDialogComponent>);
  public data: { employeeId: string; currentCTC: number } = inject(MAT_DIALOG_DATA);
  private salaryService = inject(SalaryService);
  private toast = inject(ToastService);

  form!: FormGroup;
  isLoading = signal(false);

  ngOnInit() {
    this.form = this.fb.group({
      incrementPercentage: [10, [Validators.required, Validators.min(0.1), Validators.max(100)]],
      justification: ['', [Validators.required, Validators.minLength(10)]],
      requestDate: [new Date(), Validators.required]
    });
  }

  get currentCTC(): number { return this.data.currentCTC || 0; }

  get newCTC(): number {
    const pct = this.form?.get('incrementPercentage')?.value || 0;
    return this.currentCTC * (1 + pct / 100);
  }

  get incrementAmount(): number { return this.newCTC - this.currentCTC; }

  onSubmit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isLoading.set(true);

    const payload = {
      employeeId: this.data.employeeId,
      incrementPercentage: this.form.value.incrementPercentage,
      justification: this.form.value.justification,
      requestDate: this.form.value.requestDate instanceof Date
        ? this.form.value.requestDate.toISOString()
        : this.form.value.requestDate
    };

    this.salaryService.requestIncrement(payload).subscribe({
      next: () => { this.toast.success('Increment request submitted'); this.dialogRef.close(true); },
      error: (err) => { this.toast.error(err.error?.message || 'Failed'); this.isLoading.set(false); }
    });
  }

  onCancel() { this.dialogRef.close(false); }
}
