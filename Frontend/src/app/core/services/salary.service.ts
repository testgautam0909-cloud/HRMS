import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';
import { SalaryStructure, SalaryStructureCreate, IncrementRequest, IncrementRequestCreate, IncrementApproveReject } from '../models/salary.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
    providedIn: 'root'
})
export class SalaryService {
    private api = inject(ApiService);

    createStructure(dto: SalaryStructureCreate): Observable<ApiResponse<SalaryStructure>> {
        return this.api.post<SalaryStructure>('Salary/structure', dto);
    }

    getActiveStructure(employeeId: string): Observable<ApiResponse<SalaryStructure>> {
        return this.api.get<SalaryStructure>(`Salary/structure/${employeeId}`);
    }

    getHistory(employeeId: string): Observable<ApiResponse<SalaryStructure[]>> {
        return this.api.get<SalaryStructure[]>(`Salary/structure/${employeeId}/history`);
    }

    requestIncrement(dto: IncrementRequestCreate): Observable<ApiResponse<IncrementRequest>> {
        return this.api.post<IncrementRequest>('Salary/increment', dto);
    }

    approveRejectIncrement(id: string, dto: IncrementApproveReject): Observable<ApiResponse<IncrementRequest>> {
        return this.api.put<IncrementRequest>(`Salary/increment/${id}/approve-reject`, dto);
    }

    getIncrements(employeeId?: string, page?: number, pageSize?: number): Observable<any> {
        const params: any = {};
        if (employeeId) params.employeeId = employeeId;
        if (page) params.page = page;
        if (pageSize) params.pageSize = pageSize;
        return this.api.get<any>('Salary/increments', params);
    }
}
