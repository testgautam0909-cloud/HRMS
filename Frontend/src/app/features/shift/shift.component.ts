import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShiftService } from '../../core/services/shift.service';
import { AuthService } from '../../core/services/auth.service';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusChipComponent } from '../../shared/components/status-chip/status-chip.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-shift',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    PageHeaderComponent,
    StatusChipComponent,
    EmptyStateComponent
  ],
  templateUrl: './shift.component.html',
  styleUrl: './shift.component.css'
})
export class ShiftComponent implements OnInit {
  private shiftService = inject(ShiftService);
  private auth = inject(AuthService);

  currentShift = signal<any>(null);
  schedule = signal<any[]>([]);

  ngOnInit() {
    this.loadSchedule();
  }

  loadSchedule() {
    const user = this.auth.currentUser();
    const empId = user?.employeeId;
    if (!empId) return;

    this.shiftService.getEmployeeSchedule(empId).subscribe({
      next: (res: any) => {
        const data = res.data; // This is EmployeeShiftScheduleDto

        if (data && data.shiftDetails && data.shiftDetails.length > 0) {
          const today = new Date().toDateString();

          // Find the active shift for TODAY specifically
          const activeToday = data.shiftDetails.find((s: any) =>
            s.isActive && new Date(s.assignmentDate).toDateString() === today
          );

          this.currentShift.set(activeToday || null);

          // Map all details to the schedule
          this.schedule.set(data.shiftDetails.map((s: any) => ({
            ...s,
            date: s.assignmentDate,
            isToday: new Date(s.assignmentDate).toDateString() === today
          })));
        } else {
          this.currentShift.set(null);
          this.schedule.set([]);
        }
      }
    });
  }
}
