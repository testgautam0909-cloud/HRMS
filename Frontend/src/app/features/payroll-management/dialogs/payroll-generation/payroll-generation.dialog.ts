import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { debounceTime, distinctUntilChanged, switchMap, of, catchError } from 'rxjs';
import { PayrollService } from '../../../../core/services/payroll.service';
import { ApiService } from '../../../../core/services/api.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-payroll-generation-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    MatAutocompleteModule
  ],
  templateUrl: './payroll-generation.dialog.html',
  styleUrl: './payroll-generation.dialog.css'
})
export class PayrollGenerationDialogComponent implements OnInit {
  private dialogRef = inject(MatDialogRef<PayrollGenerationDialogComponent>);
  private payrollService = inject(PayrollService);
  private apiService = inject(ApiService);
  private toast = inject(ToastService);

  isLoading = signal<boolean>(false);
  isSearching = signal<boolean>(false);
  specificMode = signal<boolean>(false); // false = all employees, true = specific employees

  selectedMonth = signal<number>(new Date().getMonth() + 1);
  selectedYear = signal<number>(new Date().getFullYear());

  // Employee selection
  searchControl = new FormControl('');
  searchResults = signal<any[]>([]);
  selectedEmployees = signal<{ id: string; name: string }[]>([]);

  months = [
    { value: 1, label: 'January' }, { value: 2, label: 'February' }, { value: 3, label: 'March' },
    { value: 4, label: 'April' }, { value: 5, label: 'May' }, { value: 6, label: 'June' },
    { value: 7, label: 'July' }, { value: 8, label: 'August' }, { value: 9, label: 'September' },
    { value: 10, label: 'October' }, { value: 11, label: 'November' }, { value: 12, label: 'December' }
  ];

  years = [2023, 2024, 2025, 2026];

  ngOnInit() {
    // Wire up live employee search with debounce
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => {
        if (!query || query.trim().length < 2) {
          this.searchResults.set([]);
          return of({ data: [] });
        }
        this.isSearching.set(true);
        return this.apiService.get<any[]>('employee', { search: query, pageSize: 10 }).pipe(
          catchError(() => of({ data: [] }))
        );
      })
    ).subscribe((res: any) => {
      this.searchResults.set(res?.data || []);
      this.isSearching.set(false);
    });
  }

  toggleMode(checked: boolean) {
    this.specificMode.set(checked);
    if (!checked) {
      // Clear selections when switching back to bulk
      this.selectedEmployees.set([]);
      this.searchControl.setValue('');
    }
  }

  selectEmployee(emp: any) {
    const already = this.selectedEmployees().find(e => e.id === emp.id);
    if (!already) {
      const name = `${emp.firstName || ''} ${emp.lastName || ''}`.trim() || emp.name || emp.email;
      this.selectedEmployees.update(list => [...list, { id: emp.id, name }]);
    }
    this.searchControl.setValue('', { emitEvent: false });
    this.searchResults.set([]);
  }

  removeEmployee(id: string) {
    this.selectedEmployees.update(list => list.filter(e => e.id !== id));
  }

  get canGenerate(): boolean {
    if (this.specificMode()) {
      return this.selectedEmployees().length > 0;
    }
    return true;
  }

  onGenerate() {
    if (!this.canGenerate) return;
    this.isLoading.set(true);

    const dto: { month: number; year: number; employeeIds?: string[] } = {
      month: this.selectedMonth(),
      year: this.selectedYear()
    };

    if (this.specificMode() && this.selectedEmployees().length > 0) {
      dto.employeeIds = this.selectedEmployees().map(e => e.id);
    }

    this.payrollService.generatePayroll(dto).subscribe({
      next: (res) => {
        const data = res.data;
        const count = Array.isArray(data) ? data.length : 0;
        this.toast.success(`Payroll generated for ${count} employee(s)`);
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.toast.error(err.error?.message || 'Failed to generate payroll');
        this.isLoading.set(false);
      }
    });
  }

  onCancel() {
    this.dialogRef.close(false);
  }
}
