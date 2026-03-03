import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="empty-state py-12 flex flex-col items-center justify-center text-center animate-fade-in">
      <div class="icon-container mb-4 bg-slate-50 p-6 rounded-full">
        <mat-icon [style.fontSize.px]="48" class="text-slate-300 w-12 h-12">{{ icon }}</mat-icon>
      </div>
      <h3 class="mat-headline-6 text-slate-900 mb-2">{{ title }}</h3>
      <p class="mat-body-2 text-slate-500 max-w-xs">{{ description }}</p>
    </div>
  `,
  styles: [`
    .icon-container mat-icon { width: 48px; height: 48px; }
    .animate-fade-in { animation: fadeIn 0.5s ease-out; }
    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
  `]
})
export class EmptyStateComponent {
  @Input({ required: true }) icon!: string;
  @Input({ required: true }) title!: string;
  @Input({ required: true }) description!: string;
}
