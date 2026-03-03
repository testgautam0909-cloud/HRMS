import { Component, inject, OnInit } from '@angular/core';
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
import { forkJoin, catchError, of } from 'rxjs';

@Component({
    selector: 'app-employee-form-dialog',
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
    templateUrl: './employee-form-dialog.component.html',
    styles: [`
    .form-grid {
      display: grid;
      grid-template-cols: repeat(2, 1fr);
      gap: 1.5rem;
    }
    @media (max-width: 600px) {
      .form-grid { grid-template-cols: 1fr; }
    }
  `]
})
export class EmployeeFormDialogComponent implements OnInit {
    private fb = inject(FormBuilder);
    private api = inject(ApiService);
    private dialogRef = inject(MatDialogRef<EmployeeFormDialogComponent>);
    public data = inject(MAT_DIALOG_DATA);

    employeeForm: FormGroup;
    departments: any[] = [];
    designations: any[] = [];
    employmentTypes: any[] = [];
    genders: any[] = [];
    isEdit = false;

    constructor() {
        this.isEdit = !!this.data?.employee;
        this.employeeForm = this.fb.group({
            firstName: [this.data?.employee?.firstName || '', [Validators.required]],
            lastName: [this.data?.employee?.lastName || '', [Validators.required]],
            email: [this.data?.employee?.email || '', [Validators.required, Validators.email]],
            phone: [this.data?.employee?.phone || '', [Validators.required]],
            dateOfBirth: [this.data?.employee?.dateOfBirth ? new Date(this.data.employee.dateOfBirth) : '', [Validators.required]],
            gender: [this.data?.employee?.gender || 0, [Validators.required]],
            departmentId: [this.data?.employee?.departmentId || '', [Validators.required]],
            designationId: [this.data?.employee?.designationId || '', [Validators.required]],
            employmentType: [this.data?.employee?.employmentType || 0, [Validators.required]],
            joiningDate: [this.data?.employee?.joiningDate ? new Date(this.data.employee.joiningDate) : new Date(), [Validators.required]],
            address: [this.data?.employee?.address || ''],
            emergencyContact: [this.data?.employee?.emergencyContact || '']
        });

        if (this.isEdit) {
            this.employeeForm.get('email')?.disable();
            this.employeeForm.get('firstName')?.disable();
            this.employeeForm.get('lastName')?.disable();
        }
    }

    ngOnInit() {
        this.loadDropdowns();
    }

    loadDropdowns() {
        forkJoin({
            depts: this.api.get<any[]>('employee/departments').pipe(catchError(() => of({ data: [] }))),
            desgs: this.api.get<any[]>('employee/designations').pipe(catchError(() => of({ data: [] }))),
            types: this.api.get<any[]>('employee/employment-types').pipe(catchError(() => of({ data: [] }))),
            genders: this.api.get<any[]>('employee/genders').pipe(catchError(() => of({ data: [] })))
        }).subscribe(res => {
            this.departments = res.depts.data || [];
            this.designations = res.desgs.data || [];
            this.employmentTypes = res.types.data || [];
            this.genders = res.genders.data || [];
        });
    }

    onSubmit() {
        if (this.employeeForm.invalid) return;

        const val = this.employeeForm.getRawValue();
        if (this.isEdit) {
            this.api.put(`employee/${this.data.employee.id}`, val).subscribe(res => {
                if (res.success) this.dialogRef.close(true);
            });
        } else {
            this.api.post('employee', val).subscribe(res => {
                if (res.success) this.dialogRef.close(true);
            });
        }
    }
}
