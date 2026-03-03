import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-loading-skeleton',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="skeleton-animate" [ngStyle]="styles"></div>
  `,
    styles: [`
    .skeleton-animate {
      background: linear-gradient(90deg, #f1f5f9 25%, #f8fafc 50%, #f1f5f9 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite linear;
      border-radius: 4px;
    }
    @keyframes skeleton-loading {
      0% { background-position: 200% 0; }
      100% { background-position: -200% 0; }
    }
  `]
})
export class LoadingSkeletonComponent {
    @Input() width: string = '100%';
    @Input() height: string = '20px';
    @Input() borderRadius: string = '4px';

    get styles() {
        return {
            width: this.width,
            height: this.height,
            'border-radius': this.borderRadius
        };
    }
}
