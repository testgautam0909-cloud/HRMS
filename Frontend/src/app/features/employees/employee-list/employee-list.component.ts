import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api.service';
import { EmployeeSummary, Department } from '../../../core/models/employee.model';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { EmployeeFormDialogComponent } from '../employee-form-dialog/employee-form-dialog.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { StatusChipComponent } from '../../../shared/components/status-chip/status-chip.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatMenuModule,
    MatIconModule,
    MatDialogModule,
    PageHeaderComponent,
    EmptyStateComponent,
    StatusChipComponent,
    LoadingSkeletonComponent
  ],
  templateUrl: './employee-list.component.html',
  styleUrl: './employee-list.component.css'
})
export class EmployeeListComponent implements OnInit {
  private api = inject(ApiService);
  private dialog = inject(MatDialog);

  employees = signal<EmployeeSummary[]>([]);
  departments = signal<Department[]>([]);
  isLoading = signal(true);

  searchQuery = '';
  selectedDept = '';

  ngOnInit() {
    this.loadData();
    this.loadDepartments();
  }

  loadData() {
    this.isLoading.set(true);
    this.api.get<any>('employee', { search: this.searchQuery, departmentId: this.selectedDept }).subscribe({
      next: (res) => {
        this.employees.set(res.data || []);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  loadDepartments() {
    this.api.get<Department[]>('employee/departments').subscribe({
      next: (res) => {
        // Filter out any placeholder entry named 'Departments' (HRMS-024)
        const depts = (res.data || []).filter((d: any) => d.name.toLowerCase() !== 'departments');
        this.departments.set(depts);
      }
    });
  }

  openAddDialog() {
    const dialogRef = this.dialog.open(EmployeeFormDialogComponent, {
      width: '800px',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadData();
    });
  }

  onSearch() {
    this.loadData();
  }

  onFilter() {
    this.loadData();
  }

  resetFilters() {
    this.searchQuery = '';
    this.selectedDept = '';
    this.loadData();
  }
}
