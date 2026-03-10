import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { ShiftService } from '../../../core/services/shift.service';

@Component({
  selector: 'app-shift-history-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatTableModule, MatIconModule],
  template: `
    <h2 mat-dialog-title class="!text-xl !font-black !text-slate-900 border-b pb-4 mb-0">
      Assignment History: <span class="text-primary-600">{{data.employeeName}}</span>
    </h2>
    <mat-dialog-content class="!pt-6">
      <div class="overflow-hidden rounded-2xl border border-slate-100 shadow-sm">
        <table mat-table [dataSource]="history()" class="w-full">
          <ng-container matColumnDef="date">
            <th mat-header-cell *matHeaderCellDef class="!bg-slate-50/50 !py-4 !px-6 !text-[10px] !font-black !text-slate-400 !uppercase !tracking-widest">Date</th>
            <td mat-cell *matCellDef="let h" class="!py-4 !px-6 text-slate-600 font-medium">{{h.assignmentDate | date:'mediumDate'}}</td>
          </ng-container>

          <ng-container matColumnDef="shift">
            <th mat-header-cell *matHeaderCellDef class="!bg-slate-50/50 !py-4 !px-6 !text-[10px] !font-black !text-slate-400 !uppercase !tracking-widest">Shift Type</th>
            <td mat-cell *matCellDef="let h" class="!py-4 !px-6">
                <span class="font-bold text-slate-700">{{h.shiftName}}</span>
            </td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef class="!bg-slate-50/50 !py-4 !px-6 !text-[10px] !font-black !text-slate-400 !uppercase !tracking-widest">Status</th>
            <td mat-cell *matCellDef="let h" class="!py-4 !px-6">
              <span [class]="'px-3 py-1 rounded-full text-[9px] font-black uppercase tracking-wider ' + (h.isActive ? 'bg-green-100 text-green-700' : 'bg-slate-100 text-slate-500')">
                  {{h.isActive ? 'Active' : 'Archived'}}
              </span>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns;" class="hover:bg-slate-50/50 transition-colors"></tr>
        </table>
      </div>

      <div *ngIf="history().length === 0" class="py-12 text-center text-slate-400">
        <mat-icon class="!w-12 !h-12 !text-[48px] opacity-20 mb-2">history</mat-icon>
        <p class="font-bold">No history found</p>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end" class="p-6">
      <button mat-flat-button color="primary" class="!rounded-xl" mat-dialog-close>Close Portal</button>
    </mat-dialog-actions>
  `
})
export class ShiftHistoryDialogComponent implements OnInit {
  private shiftService = inject(ShiftService);
  public data = inject(MAT_DIALOG_DATA);

  history = signal<any[]>([]);
  columns = ['date', 'shift', 'status'];

  ngOnInit() {
    this.shiftService.getAssignmentsByEmployee(this.data.employeeId).subscribe(res => {
      this.history.set(res.data);
    });
  }
}
