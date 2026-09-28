import { HttpErrorResponse } from '@angular/common/http';

// API returns an error code ("Book.HasBorrowedCopies") or a list of codes for validation failures.
export function getErrorMessage(err: HttpErrorResponse): string {
    if (err.status === 0) {
        return 'Cannot connect to server';
    }
    if (err.status === 403) {
        return 'You do not have permission to do this';
    }
    if (Array.isArray(err.error)) {
        return err.error.join(', ');
    }
    if (typeof err.error === 'string' && err.error !== '') {
        return err.error;
    }
    return 'Something went wrong';
}
