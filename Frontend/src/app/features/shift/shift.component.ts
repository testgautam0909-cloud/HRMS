import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
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
  private api = inject(ApiService);
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

    this.api.get<any>(`shift/schedule/employee/${empId}`).subscribe({
      next: (res: any) => {
        const data = res.data;
        this.currentShift.set(data.currentShift);
        this.schedule.set(data.upcomingSchedule.map((s: any) => ({
          ...s,
          isToday: new Date(s.date).toDateString() === new Date().toDateString()
        })));
      }
    });
  }
}
