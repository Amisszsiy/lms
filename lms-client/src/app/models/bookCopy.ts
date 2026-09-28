// Numbers must match BookCopyStatus in the API. Enums are sent as numbers.
export enum BookCopyStatus {
    Available = 1,
    Borrowed = 2,
    Damaged = 3,
    Lost = 4,
    Withdrawn = 5,
    IssueReturn = 6
}

export function getBookCopyStatusLabel(status: BookCopyStatus): string {
    switch (status) {
        case BookCopyStatus.Available: return 'Available';
        case BookCopyStatus.Borrowed: return 'Borrowed';
        case BookCopyStatus.Damaged: return 'Damaged';
        case BookCopyStatus.Lost: return 'Lost';
        case BookCopyStatus.Withdrawn: return 'Withdrawn';
        case BookCopyStatus.IssueReturn: return 'Return Requested';
        default: return '';
    }
}

// Same rule as BookCopy.IsOnLoan in the API.
export function isBookCopyOnLoan(status: BookCopyStatus): boolean {
    return status === BookCopyStatus.Borrowed || status === BookCopyStatus.IssueReturn;
}

export interface BookCopyResponse {
    id: string;
    bookId: number;
    locationId: number;
    availableStatus: BookCopyStatus;
}

export interface CreateBookCopyRequest {
    bookId: number;
    locationId: number;
}

export interface UpdateBookCopyRequest {
    locationId: number;
    status: BookCopyStatus;
}
