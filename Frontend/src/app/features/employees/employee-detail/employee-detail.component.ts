import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { EmployeeFormDialogComponent } from '../employee-form-dialog/employee-form-dialog.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { SalaryStructureDialogComponent } from '../dialogs/salary-structure/salary-structure.dialog';
import { IncrementRequestDialogComponent } from '../dialogs/increment-request/increment-request.dialog';
import { StatusChipComponent } from '../../../shared/components/status-chip/status-chip.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { SalaryService } from '../../../core/services/salary.service';
import { SalaryStructure, IncrementRequest } from '../../../core/models/salary.model';
import { finalize, forkJoin } from 'rxjs';

@Component({
  selector: 'app-employee-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatTabsModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    PageHeaderComponent,
    StatusChipComponent,
    LoadingSkeletonComponent,
    EmptyStateComponent,
    MatDividerModule
  ],
  templateUrl: './employee-detail.component.html',
  styleUrl: './employee-detail.component.css'
})
export class EmployeeDetailComponent implements OnInit {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);
  private salaryService = inject(SalaryService);

  employee = signal<any>(null);
  activeSalary = signal<SalaryStructure | null>(null);
  salaryHistory = signal<SalaryStructure[]>([]);
  increments = signal<IncrementRequest[]>([]);
  isLoading = signal(true);
  isSalaryLoading = signal(false);

  get personalDetails() {
    const emp = this.employee();
    if (!emp) return [];

    return [
      { label: 'Full Name', value: `${emp.firstName} ${emp.lastName}`, icon: 'person' },
      { label: 'Email', value: emp.email, icon: 'mail_outline' },
      { label: 'Phone', value: emp.phone, icon: 'phone' },
      { label: 'Gender', value: emp.gender, icon: 'wc' },
      { label: 'Birth Date', value: new Date(emp.dateOfBirth).toLocaleDateString(), icon: 'cake' },
      { label: 'Address', value: emp.address || 'N/A', icon: 'location_on' },
      { label: 'Emergency Contact', value: emp.emergencyContact || 'N/A', icon: 'contact_phone' },
    ];
  }

  get workDetails() {
    const emp = this.employee();
    if (!emp) return [];

    return [
      { label: 'Employee Code', value: emp.employeeCode, icon: 'badge' },
      { label: 'Department', value: emp.department, icon: 'domain' },
      { label: 'Designation', value: emp.designation, icon: 'business_center' },
      { label: 'Employment Type', value: emp.employmentType, icon: 'work_outline' },
      { label: 'Joining Date', value: new Date(emp.joiningDate).toLocaleDateString(), icon: 'event_available' },
      { label: 'Shift', value: emp.currentShift || 'Not Assigned', icon: 'schedule' },
    ];
  }

  ngOnInit() {
    this.loadProfile();
  }

  loadProfile() {
    this.isLoading.set(true);
    const id = this.route.snapshot.params['id'];
    
    this.api.get<any>(`employee/${id}/profile`)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (res) => {
          this.employee.set(res.data);
          this.loadSalaryData(id);
        }
      });
  }

  loadSalaryData(employeeId: string) {
    this.isSalaryLoading.set(true);
    forkJoin({
      active: this.salaryService.getActiveStructure(employeeId),
      history: this.salaryService.getHistory(employeeId),
      increments: this.salaryService.getIncrements(employeeId)
    }).pipe(finalize(() => this.isSalaryLoading.set(false)))
    .subscribe({
      next: (res) => {
        this.activeSalary.set(res.active.data);
        this.salaryHistory.set(res.history.data);
        this.increments.set(res.increments.data);
      }
    });
  }

  openEditDialog() {
    const dialogRef = this.dialog.open(EmployeeFormDialogComponent, {
      width: '800px',
      data: { employee: this.employee() },
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadProfile();
    });
  }

  openSalaryDialog() {
    this.dialog.open(SalaryStructureDialogComponent, {
      width: '640px',
      data: {
        employeeId: this.employee().id,
        currentStructure: this.activeSalary()
      },
      panelClass: 'custom-dialog-container'
    }).afterClosed().subscribe(result => {
      if (result) {
        this.loadSalaryData(this.employee().id);
      }
    });
  }

  openIncrementDialog() {
    this.dialog.open(IncrementRequestDialogComponent, {
      width: '520px',
      data: {
        employeeId: this.employee().id,
        currentCTC: this.activeSalary()?.ctc || 0
      },
      panelClass: 'custom-dialog-container'
    }).afterClosed().subscribe(result => {
      if (result) {
        this.loadSalaryData(this.employee().id);
      }
    });
  }
}
