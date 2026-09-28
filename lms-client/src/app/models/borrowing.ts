// Numbers must match BorrowingStatus in the API.
export enum BorrowingStatus {
    Borrowed = 1,
    Returned = 2,
    Overdue = 3,
    ReturnRequested = 4
}

export function getBorrowingStatusLabel(status: BorrowingStatus): string {
    switch (status) {
        case BorrowingStatus.Borrowed: return 'Borrowed';
        case BorrowingStatus.Returned: return 'Returned';
        case BorrowingStatus.Overdue: return 'Overdue';
        case BorrowingStatus.ReturnRequested: return 'Return Requested';
        default: return '';
    }
}

export interface BorrowingUserResponse {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
}

export interface BorrowingBookResponse {
    id: number;
    isbn: string;
    title: string;
}

export interface BorrowingResponse {
    id: string;
    userId: string;
    bookCopyId: string;
    user: BorrowingUserResponse;
    book: BorrowingBookResponse;
    borrowDate: string;
    dueDate: string;
    returnDate: string | null;
    status: BorrowingStatus;
}

export interface SearchBorrowingsRequest {
    search: string;
    status: BorrowingStatus | null;
    pageNo: number;
    pageSize: number;
}

export interface BorrowRequest {
    bookCopyId: string;
}

export interface AssignBorrowRequest {
    userId: string;
    bookCopyId: string;
}
