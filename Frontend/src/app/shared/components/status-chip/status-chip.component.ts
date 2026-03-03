import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';

export type StatusType = 'success' | 'warning' | 'error' | 'info' | 'primary' | 'neutral';

@Component({
    selector: 'app-status-chip',
    standalone: true,
    imports: [CommonModule, MatChipsModule],
    template: `
    <span class="status-chip px-3 py-1 rounded-full text-xs font-bold leading-none border"
          [ngClass]="colorClass">
      {{ label }}
    </span>
  `,
    styles: [`
    .status-chip {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      white-space: nowrap;
    }
    .status-success { background: #f0fdf4; color: #16a34a; border-color: #dcfce7; }
    .status-warning { background: #fffbeb; color: #d97706; border-color: #fef3c7; }
    .status-error { background: #fef2f2; color: #dc2626; border-color: #fee2e2; }
    .status-info { background: #eff6ff; color: #2563eb; border-color: #dbeafe; }
    .status-primary { background: #f5f3ff; color: #4f46e5; border-color: #ede9fe; }
    .status-neutral { background: #f8fafc; color: #64748b; border-color: #f1f5f9; }
  `]
})
export class StatusChipComponent {
    @Input({ required: true }) label!: string;
    @Input() type: StatusType = 'neutral';

    get colorClass(): string {
        return `status-${this.type}`;
    }
}
