import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-header mb-8 pt-3">
      <div class="flex items-center justify-between flex-wrap gap-4">
        <div>
          <h1 class="mat-headline-4 text-slate-900 font-bold mb-1">{{ title }}</h1>
          <p class="mat-body-2 text-slate-500 font-medium" *ngIf="subtitle">{{ subtitle }}</p>
        </div>
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [`
    .page-header {
      border-bottom: 1px solid rgba(0,0,0,0.05);
      padding-bottom: 1rem;
    }
  `]
})
export class PageHeaderComponent {
  @Input({ required: true }) title!: string;
  @Input() subtitle?: string;
}
