import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { ShiftService } from '../../core/services/shift.service';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ToastService } from '../../core/services/toast.service';
import { Shift } from '../../core/models/shift.model';
import { ShiftDialogComponent } from './shift-dialog/shift-dialog.component';
import { ShiftAssignmentDialogComponent } from './shift-assignment-dialog/shift-assignment-dialog.component';
import { ShiftHistoryDialogComponent } from './shift-history-dialog/shift-history-dialog.component';

@Component({
    selector: 'app-shift-management',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule,
        MatTableModule,
        MatDialogModule,
        MatMenuModule,
        MatTooltipModule,
        MatTabsModule,
        PageHeaderComponent
    ],
    templateUrl: './shift-management.component.html',
    styleUrl: './shift-management.component.css'
})
export class ShiftManagementComponent implements OnInit {
    private shiftService = inject(ShiftService);
    public toast = inject(ToastService);
    private dialog = inject(MatDialog);

    shifts = signal<Shift[]>([]);
    assignments = signal<any[]>([]);
    searchTerm = signal<string>('');

    filteredAssignments = computed(() => {
        const term = this.searchTerm().toLowerCase();
        return this.assignments().filter(a =>
            a.employeeName?.toLowerCase().includes(term) ||
            a.shiftName?.toLowerCase().includes(term)
        );
    });

    // Statistics
    totalShifts = signal<number>(0);
    activeAssignments = signal<number>(0);

    displayedColumns: string[] = ['name', 'startTime', 'endTime', 'actions'];
    assignmentColumns: string[] = ['employeeName', 'shiftName', 'date', 'status', 'actions'];

    ngOnInit() {
        this.loadData();
    }

    loadData() {
        this.shiftService.getAllShifts().subscribe({
            next: (res) => {
                this.shifts.set(res.data);
                this.totalShifts.set(res.data.length);
            },
            error: () => this.toast.error('Failed to load shifts')
        });

        this.shiftService.getShiftAssignments().subscribe({
            next: (res) => {
                this.assignments.set(res.data);
                this.activeAssignments.set(res.data.filter((a: any) => a.isActive).length);
            },
            error: () => this.toast.error('Failed to load assignments')
        });
    }

    openShiftDialog(shift?: Shift) {
        const dialogRef = this.dialog.open(ShiftDialogComponent, {
            width: '550px',
            data: { shift },
            panelClass: 'premium-dialog'
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                const action = result.id ? this.shiftService.updateShift(result) : this.shiftService.createShift(result);
                action.subscribe({
                    next: () => {
                        this.toast.success(`Shift ${result.id ? 'updated' : 'created'} successfully`);
                        this.loadData();
                    },
                    error: (err) => this.toast.error(err.error?.message || 'Action failed')
                });
            }
        });
    }

    openAssignDialog() {
        const dialogRef = this.dialog.open(ShiftAssignmentDialogComponent, {
            width: '650px',
            data: { shifts: this.shifts() },
            panelClass: 'premium-dialog'
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.shiftService.assignShift(result).subscribe({
                    next: () => {
                        this.toast.success('Shift assigned successfully');
                        this.loadData();
                    },
                    error: (err) => this.toast.error(err.error?.message || 'Assignment failed')
                });
            }
        });
    }

    viewHistory(employeeId: string, employeeName: string) {
        this.dialog.open(ShiftHistoryDialogComponent, {
            width: '600px',
            data: { employeeId, employeeName },
            panelClass: 'premium-dialog'
        });
    }

    deleteShift(id: string) {
        if (confirm('Are you sure you want to delete this shift?')) {
            this.shiftService.deleteShift(id).subscribe({
                next: () => {
                    this.toast.success('Shift deleted successfully');
                    this.loadData();
                },
                error: (err) => this.toast.error(err.error?.message || 'Failed to delete shift')
            });
        }
    }

    deleteAssignment(id: string) {
        if (confirm('Are you sure you want to remove this assignment?')) {
            this.shiftService.deleteAssignment(id).subscribe({
                next: () => {
                    this.toast.success('Assignment removed successfully');
                    this.loadData();
                },
                error: (err) => this.toast.error(err.error?.message || 'Failed to remove assignment')
            });
        }
    }
}
