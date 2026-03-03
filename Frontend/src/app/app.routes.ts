import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';

export const routes: Routes = [
    {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
    },
    {
        path: '',
        component: MainLayoutComponent,
        canActivate: [authGuard],
        children: [
            {
                path: 'dashboard',
                loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
            },
            {
                path: 'employees',
                children: [
                    {
                        path: '',
                        loadComponent: () => import('./features/employees/employee-list/employee-list.component').then(m => m.EmployeeListComponent)
                    },
                    {
                        path: ':id',
                        loadComponent: () => import('./features/employees/employee-detail/employee-detail.component').then(m => m.EmployeeDetailComponent)
                    }
                ]
            },
            {
                path: 'attendance',
                loadComponent: () => import('./features/attendance/attendance.component').then(m => m.AttendanceComponent)
            },
            {
                path: 'leave',
                loadComponent: () => import('./features/leave/leave.component').then(m => m.LeaveComponent)
            },
            {
                path: 'payroll',
                loadComponent: () => import('./features/payroll/payroll.component').then(m => m.PayrollComponent)
            },
            {
                path: 'shift',
                loadComponent: () => import('./features/shift/shift.component').then(m => m.ShiftComponent)
            },
            {
                path: 'chat',
                loadComponent: () => import('./features/chat/chat.component').then(m => m.ChatComponent)
            },
            {
                path: '',
                redirectTo: 'dashboard',
                pathMatch: 'full'
            }
        ]
    },
    {
        path: '**',
        redirectTo: 'login'
    }
];
