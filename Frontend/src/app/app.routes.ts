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
                loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
                data: { breadcrumb: 'Dashboard' }
            },
            {
                path: 'employees',
                data: { breadcrumb: 'Employees' },
                children: [
                    {
                        path: '',
                        loadComponent: () => import('./features/employees/employee-list/employee-list.component').then(m => m.EmployeeListComponent)
                    },
                    {
                        path: ':id',
                        loadComponent: () => import('./features/employees/employee-detail/employee-detail.component').then(m => m.EmployeeDetailComponent),
                        data: { breadcrumb: 'Details' }
                    }
                ]
            },
            {
                path: 'attendance',
                loadComponent: () => import('./features/attendance/attendance.component').then(m => m.AttendanceComponent),
                data: { breadcrumb: 'Attendance' }
            },
            {
                path: 'leave',
                loadComponent: () => import('./features/leave/leave.component').then(m => m.LeaveComponent),
                data: { breadcrumb: 'Leave' }
            },
            {
                path: 'payroll',
                loadComponent: () => import('./features/payroll/payroll.component').then(m => m.PayrollComponent),
                data: { breadcrumb: 'Payroll' }
            },
            {
                path: 'shift',
                loadComponent: () => import('./features/shift/shift.component').then(m => m.ShiftComponent),
                data: { breadcrumb: 'My Shift' }
            },
            {
                path: 'shift-management',
                loadComponent: () => import('./features/shift-management/shift-management.component').then(m => m.ShiftManagementComponent),
                data: { breadcrumb: 'Shift Master', roles: ['Admin', 'HR'] }
            },
            {
                path: 'chat',
                loadComponent: () => import('./features/chat/chat.component').then(m => m.ChatComponent),
                data: { breadcrumb: 'Chat' }
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
