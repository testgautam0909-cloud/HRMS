import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, NavigationEnd, Router, RouterModule } from '@angular/router';
import { filter, distinctUntilChanged } from 'rxjs/operators';
import { MatIconModule } from '@angular/material/icon';

export interface Breadcrumb {
    label: string;
    url: string;
}

@Component({
    selector: 'app-breadcrumb',
    standalone: true,
    imports: [CommonModule, RouterModule, MatIconModule],
    template: `
    <nav class="flex px-5 py-3 text-slate-400" aria-label="Breadcrumb">
      <ol class="inline-flex items-center space-x-1 md:space-x-3">
        <li class="inline-flex items-center">
          <a routerLink="/" class="inline-flex items-center text-sm font-bold hover:text-primary-600 transition-colors">
            <mat-icon class="mr-2 text-lg">home</mat-icon>
            Home
          </a>
        </li>
        <li *ngFor="let breadcrumb of breadcrumbs; let last = last">
          <div class="flex items-center">
            <mat-icon class="text-slate-300 mx-1">chevron_right</mat-icon>
            <a [routerLink]="breadcrumb.url"
               class="ml-1 text-sm font-bold hover:text-primary-600 md:ml-2 transition-colors"
               [ngClass]="{'text-primary-600': last, 'text-slate-400': !last}">
              {{ breadcrumb.label }}
            </a>
          </div>
        </li>
      </ol>
    </nav>
  `,
    styles: [`
    :host { display: block; }
  `]
})
export class BreadcrumbComponent implements OnInit {
    private router = inject(Router);
    private activatedRoute = inject(ActivatedRoute);

    breadcrumbs: Breadcrumb[] = [];

    ngOnInit() {
        this.router.events.pipe(
            filter(event => event instanceof NavigationEnd),
            distinctUntilChanged()
        ).subscribe(() => {
            this.breadcrumbs = this.createBreadcrumbs(this.activatedRoute.root);
        });

        // Initial load
        this.breadcrumbs = this.createBreadcrumbs(this.activatedRoute.root);
    }

    private createBreadcrumbs(route: ActivatedRoute, url: string = '', breadcrumbs: Breadcrumb[] = []): Breadcrumb[] {
        const children: ActivatedRoute[] = route.children;

        if (children.length === 0) {
            return breadcrumbs;
        }

        for (const child of children) {
            const routeURL: string = child.snapshot.url.map(segment => segment.path).join('/');
            if (routeURL !== '') {
                url += `/${routeURL}`;
            }

            const label = child.snapshot.data['breadcrumb'] || routeURL;
            if (label) {
                breadcrumbs.push({ label, url });
            }

            return this.createBreadcrumbs(child, url, breadcrumbs);
        }

        return breadcrumbs;
    }
}
