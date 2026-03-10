import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';
import { PayrollRecord, PayrollSummary, PayrollGenerateResult, PayrollOverrideRequest } from '../models/payroll.model';
import { ApiResponse, PagedResponse } from '../models/api-response.model';

@Injectable({
    providedIn: 'root'
})
export class PayrollService {
    private api = inject(ApiService);

    generatePayroll(dto: { month: number, year: number, employeeIds?: string[] }): Observable<ApiResponse<PayrollRecord[]>> {
        return this.api.post<PayrollRecord[]>('Payroll/generate', dto);
    }

    getById(id: string): Observable<ApiResponse<PayrollRecord>> {
        return this.api.get<PayrollRecord>(`Payroll/${id}`);
    }

    getByEmployee(employeeId: string, month?: number, year?: number): Observable<ApiResponse<PayrollRecord[]>> {
        const params: any = {};
        if (month) params.month = month;
        if (year) params.year = year;
        return this.api.get<PayrollRecord[]>(`Payroll/employee/${employeeId}`, params);
    }

    getAll(params: { employeeId?: string, month?: number, year?: number, page?: number, pageSize?: number }): Observable<any> {
        return this.api.get<any>('Payroll', params);
    }

    // Backend takes NO body — just PUT with id
    markAsPaid(id: string): Observable<ApiResponse<PayrollRecord>> {
        return this.api.put<PayrollRecord>(`Payroll/${id}/pay`, {});
    }

    // Backend takes PayrollOverrideDto {BonusAmount?, PenaltyAmount?, OverrideReason?}
    overridePayroll(id: string, dto: PayrollOverrideRequest): Observable<ApiResponse<PayrollRecord>> {
        return this.api.put<PayrollRecord>(`Payroll/${id}/override`, dto);
    }

    // Backend takes NO body
    relockPayroll(id: string): Observable<ApiResponse<PayrollRecord>> {
        return this.api.put<PayrollRecord>(`Payroll/${id}/relock`, {});
    }

    getSummary(month: number, year: number): Observable<ApiResponse<PayrollSummary>> {
        return this.api.get<PayrollSummary>('Payroll/summary', { month, year });
    }

    getSalarySlipUrl(id: string): string {
        return `${(this.api as any).baseUrl}/Payroll/${id}/salary-slip`;
    }

    uploadSalarySlip(id: string): Observable<ApiResponse<any>> {
        return this.api.post<any>(`Payroll/${id}/salary-slip/upload`, {});
    }
}
