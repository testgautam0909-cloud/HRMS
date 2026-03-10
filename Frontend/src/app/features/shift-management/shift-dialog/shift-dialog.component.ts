import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { Shift } from '../../../core/models/shift.model';

@Component({
  selector: 'app-shift-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule
  ],
  templateUrl: './shift-dialog.component.html',
})
export class ShiftDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<ShiftDialogComponent>);
  public data = inject(MAT_DIALOG_DATA);

  shiftForm: FormGroup;
  isEdit = false;

  constructor() {
    this.isEdit = !!this.data?.shift;
    this.shiftForm = this.fb.group({
      name: [this.data?.shift?.name || '', [Validators.required]],
      startTime: [this.data?.shift?.startTime || '', [Validators.required]],
      endTime: [this.data?.shift?.endTime || '', [Validators.required]],
      isDefault: [this.data?.shift?.isDefault || false]
    });
  }

  onSubmit() {
    if (this.shiftForm.invalid) return;
    const value = this.shiftForm.value;
    if (this.isEdit) {
      value.id = this.data.shift.id;
    }
    this.dialogRef.close(value);
  }
}
