import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatListModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule
  ],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent {
  private auth = inject(AuthService);
  router = inject(Router);

  readonly navItems = [
    { label: 'Dashboard', path: '/dashboard', icon: 'dashboard', tooltip: 'Overview & Statistics' },
    { label: 'Employees', path: '/employees', icon: 'badge', roles: ['Admin', 'HR'], tooltip: 'Staff Management' },
    { label: 'Attendance', path: '/attendance', icon: 'event_available', tooltip: 'Punch in/out & History' },
    { label: 'Leave', path: '/leave', icon: 'event_note', tooltip: 'Request time off' },
    { label: 'Payroll', path: '/payroll', icon: 'payments', tooltip: 'Salary & Slips' },
    { label: 'My Shift', path: '/shift', icon: 'schedule', tooltip: 'Your Work Roster' },
    { label: 'Shift Master', path: '/shift-management', icon: 'settings_applications', roles: ['Admin', 'HR'], tooltip: 'Manage Company Shifts' },
    { label: 'Chat', path: '/chat', icon: 'forum', tooltip: 'Internal Communication' },
  ];

  hasRole(roles: string[]): boolean {
    const user = this.auth.currentUser();
    return !user || roles.includes(user.role);
  }

  logout() {
    this.auth.logout();
  }
}
