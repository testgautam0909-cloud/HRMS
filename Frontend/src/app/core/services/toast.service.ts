import { Injectable, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

@Injectable({
    providedIn: 'root'
})
export class ToastService {
    private snackBar = inject(MatSnackBar);

    private readonly defaultConfig: MatSnackBarConfig = {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'bottom',
    };

    success(message: string, action: string = 'Close') {
        this.snackBar.open(message, action, {
            ...this.defaultConfig,
            panelClass: ['toast-success']
        });
    }

    error(message: string, action: string = 'Close') {
        this.snackBar.open(message, action, {
            ...this.defaultConfig,
            panelClass: ['toast-error'],
            duration: 7000
        });
    }

    info(message: string, action: string = 'Close') {
        this.snackBar.open(message, action, {
            ...this.defaultConfig,
            panelClass: ['toast-info']
        });
    }

    warning(message: string, action: string = 'Close') {
        this.snackBar.open(message, action, {
            ...this.defaultConfig,
            panelClass: ['toast-warning']
        });
    }
}
