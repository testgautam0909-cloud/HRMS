import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { ApiService } from '../../../core/services/api.service';
import { Shift } from '../../../core/models/shift.model';


@Component({
  selector: 'app-shift-assignment-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule
  ],
  templateUrl: './shift-assignment-dialog.component.html',
  styleUrls: ['./shift-assignment-dialog.component.css']
})
export class ShiftAssignmentDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(ApiService);
  private dialogRef = inject(MatDialogRef<ShiftAssignmentDialogComponent>);
  public data = inject(MAT_DIALOG_DATA);

  assignForm: FormGroup;
  employees = signal<any[]>([]);
  shifts: Shift[] = [];

  constructor() {
    this.shifts = this.data?.shifts || [];
    this.assignForm = this.fb.group({
      employeeId: ['', [Validators.required]],
      shiftId: ['', [Validators.required]],
      assignmentDate: [new Date(), [Validators.required]],
      reason: ['']
    });
  }

  ngOnInit() {
    this.loadEmployees();


  }

  loadEmployees() {
    this.api.get<any[]>('employee').subscribe(res => {
      this.employees.set(res.data);
    });
  }

  getInitials(name: string): string {
    if (!name) return '??';
    const parts = name.split(' ').filter(p => p.length > 0);
    if (parts.length === 0) return '??';
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  }

  onSubmit() {
    if (this.assignForm.invalid) return;
    this.dialogRef.close(this.assignForm.value);
  }
}
