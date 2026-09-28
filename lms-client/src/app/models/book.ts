export interface BookResponse {
    id: number;
    isbn: string;
    title: string;
    publisherId: number;
    publicationYear: number;
    isActive: boolean;
    authorIds: number[];
    genreIds: number[];
    availableCopyCount: number;
}

export interface SaveBookRequest {
    isbn: string;
    title: string;
    publisherId: number;
    publicationYear: number;
    authorIds: number[];
    genreIds: number[];
}

export interface SearchBooksRequest {
    search: string;
    includeInactive: boolean;
    pageNo: number;
    pageSize: number;
}
