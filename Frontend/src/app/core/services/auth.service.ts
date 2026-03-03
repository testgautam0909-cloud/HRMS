import { Injectable, signal } from '@angular/core';
import { ApiService } from './api.service';
import { Observable, tap, of } from 'rxjs';
import { LoginResponse, User } from '../models/auth.model';
import { Router } from '@angular/router';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly TOKEN_KEY = 'hrms_token';
    private readonly REFRESH_TOKEN_KEY = 'hrms_refresh_token';
    private readonly USER_KEY = 'hrms_user';

    currentUser = signal<User | null>(this.getStoredUser());

    constructor(private api: ApiService, private router: Router) { }

    login(credentials: any): Observable<any> {
        return this.api.post<LoginResponse>('auth/login', credentials).pipe(
            tap(res => {
                if (res.success) {
                    this.setSession(res.data);
                }
            })
        );
    }

    refreshToken(): Observable<any> {
        const refreshToken = this.getRefreshToken();
        if (!refreshToken) return of({ success: false });

        return this.api.post<LoginResponse>('auth/refresh', { refreshToken }).pipe(
            tap(res => {
                if (res.success) {
                    this.setSession(res.data);
                } else {
                    this.logout();
                }
            })
        );
    }

    logout() {
        const refreshToken = this.getRefreshToken();
        if (refreshToken) {
            this.api.post('auth/logout', { refreshToken }).subscribe();
        }

        localStorage.removeItem(this.TOKEN_KEY);
        localStorage.removeItem(this.REFRESH_TOKEN_KEY);
        localStorage.removeItem(this.USER_KEY);
        this.currentUser.set(null);
        this.router.navigate(['/login']);
    }

    getToken(): string | null {
        return localStorage.getItem(this.TOKEN_KEY);
    }

    getRefreshToken(): string | null {
        return localStorage.getItem(this.REFRESH_TOKEN_KEY);
    }

    private setSession(authResult: LoginResponse) {
        localStorage.setItem(this.TOKEN_KEY, authResult.accessToken);
        localStorage.setItem(this.REFRESH_TOKEN_KEY, authResult.refreshToken);

        const user: User = {
            id: authResult.userId,
            email: authResult.email,
            role: authResult.role,
            fullName: authResult.fullName,
            employeeId: authResult.employeeId
        };

        localStorage.setItem(this.USER_KEY, JSON.stringify(user));
        this.currentUser.set(user);
    }

    private getStoredUser(): User | null {
        const userJson = localStorage.getItem(this.USER_KEY);
        return userJson ? JSON.parse(userJson) : null;
    }

    isLoggedIn(): boolean {
        return !!this.getToken();
    }

    hasRole(role: string): boolean {
        return this.currentUser()?.role === role;
    }
}
