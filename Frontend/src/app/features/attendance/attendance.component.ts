import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { format } from 'date-fns';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { AttendanceRecord, AttendanceSummary } from '../../core/models/attendance.model';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusChipComponent, StatusType } from '../../shared/components/status-chip/status-chip.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { finalize } from 'rxjs';

@Component({
    selector: 'app-attendance',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatTableModule,
        PageHeaderComponent,
        StatusChipComponent,
        EmptyStateComponent
    ],
    templateUrl: './attendance.component.html',
    styleUrl: './attendance.component.css'
})
export class AttendanceComponent implements OnInit, OnDestroy {
    private api = inject(ApiService);
    private auth = inject(AuthService);

    currentTime = new Date();
    isCheckedIn = signal(false);
    isLoading = signal(false);
    lastActionTime = signal<Date | null>(null);
    records = signal<AttendanceRecord[]>([]);
    summary = signal<AttendanceSummary | null>(null);

    stats = [
        { label: 'Total Present', value: '18 Days' },
        { label: 'Avg In-Time', value: '09:05 AM' },
        { label: 'Total Hours', value: '142h' },
    ];

    private intervalId: any;

    constructor() {
        this.intervalId = setInterval(() => this.currentTime = new Date(), 1000);
    }

    ngOnInit() {
        this.loadHistory();
    }

    ngOnDestroy(): void {
        if (this.intervalId) {
            clearInterval(this.intervalId);
        }
    }

    loadHistory() {
        const user = this.auth.currentUser();
        const empId = user?.employeeId;
        if (!empId) return;

        const month = new Date().getMonth() + 1;
        const year = new Date().getFullYear();

        this.api.get<AttendanceSummary>(`attendance/monthly/${empId}`, { month, year }).subscribe({
            next: (res: any) => {
                const data = res.data;
                if (data) {
                    this.records.set(data.records || []);
                    this.summary.set(data);
                    const today = format(new Date(), 'yyyy-MM-dd');
                    const todayRecords = (data.records || []).filter((r: AttendanceRecord) => format(new Date(r.date), 'yyyy-MM-dd') === today);

                    // Find if there's any record that is currently "open" (no check-out)
                    const openRecord = todayRecords.find((r: AttendanceRecord) => !r.checkOutTime);

                    if (openRecord) {
                        this.isCheckedIn.set(true);
                        this.lastActionTime.set(new Date(openRecord.checkInTime));
                    } else {
                        this.isCheckedIn.set(false);
                        // If we have records today but none are open, show the last checkout time
                        if (todayRecords.length > 0) {
                            const lastClosed = [...todayRecords].sort((a, b) =>
                                new Date(b.checkOutTime!).getTime() - new Date(a.checkOutTime!).getTime())[0];
                            this.lastActionTime.set(new Date(lastClosed.checkOutTime!));
                        }
                    }
                }
            }
        });
    }

    checkIn() {
        this.isLoading.set(true);
        this.api.post<any>('attendance/check-in', { date: new Date() }).pipe(
            finalize(() => this.isLoading.set(false))
        ).subscribe({
            next: (res: any) => {
                this.isCheckedIn.set(true);
                this.lastActionTime.set(new Date());
                this.loadHistory();
            },
            error: () => { }
        });
    }

    checkOut() {
        this.isLoading.set(true);
        this.api.post<any>('attendance/check-out', { date: new Date() }).pipe(
            finalize(() => this.isLoading.set(false))
        ).subscribe({
            next: (res: any) => {
                this.isCheckedIn.set(false);
                this.lastActionTime.set(new Date());
                this.loadHistory();
            },
            error: () => { }
        });
    }

    getStatusType(status: string): StatusType {
        switch (status.toLowerCase()) {
            case 'present': return 'success';
            case 'late': return 'warning';
            case 'absent': return 'error';
            case 'onleave': return 'info';
            default: return 'neutral';
        }
    }
}
