import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Shift, ShiftSchedule } from '../models/shift.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
    providedIn: 'root'
})
export class ShiftService {
    private api = inject(ApiService);

    getAllShifts(): Observable<ApiResponse<Shift[]>> {
        return this.api.get<Shift[]>('shift');
    }

    getShiftById(id: string): Observable<ApiResponse<Shift>> {
        return this.api.get<Shift>(`shift/${id}`);
    }

    createShift(shift: any): Observable<ApiResponse<Shift>> {
        return this.api.post<Shift>('shift', shift);
    }

    updateShift(shift: any): Observable<ApiResponse<Shift>> {
        return this.api.put<Shift>('shift', shift);
    }

    deleteShift(id: string): Observable<ApiResponse<void>> {
        return this.api.delete<void>(`shift/${id}`);
    }

    getShiftAssignments(): Observable<ApiResponse<any[]>> {
        return this.api.get<any[]>('shift/assignments');
    }

    getAssignmentsByEmployee(employeeId: string): Observable<ApiResponse<any[]>> {
        return this.api.get<any[]>(`shift/assignments/employee/${employeeId}`);
    }

    assignShift(assignment: any): Observable<ApiResponse<any>> {
        return this.api.post<any>('shift/assign', assignment);
    }

    getEmployeeSchedule(employeeId: string): Observable<ApiResponse<any>> {
        return this.api.get<any>(`shift/schedule/employee/${employeeId}`);
    }

    deleteAssignment(id: string): Observable<ApiResponse<void>> {
        return this.api.delete<void>(`shift/assignments/${id}`);
    }
}
