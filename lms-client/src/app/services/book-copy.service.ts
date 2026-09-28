import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { BookCopyResponse, CreateBookCopyRequest, UpdateBookCopyRequest } from '../models/bookCopy';

@Injectable({ providedIn: 'root' })
export class BookCopyService {
    private readonly baseUrl = environment.apiUrl;
    constructor(private http: HttpClient) {}

    // API has no paging here: it returns every copy of one book.
    getBookCopies(bookId: number, includeWithdrawn: boolean) {
        let params = new HttpParams();
        params = params.set('bookId', bookId);
        params = params.set('includeWithdrawn', includeWithdrawn);

        return this.http.get<BookCopyResponse[]>(`${this.baseUrl}/api/bookcopy`, { params: params });
    }

    addBookCopy(request: CreateBookCopyRequest) {
        return this.http.post<BookCopyResponse>(`${this.baseUrl}/api/bookcopy`, request);
    }

    updateBookCopy(id: string, request: UpdateBookCopyRequest) {
        return this.http.put<BookCopyResponse>(`${this.baseUrl}/api/bookcopy/${id}`, request);
    }

    // Soft delete: status becomes Withdrawn.
    withdrawBookCopy(id: string) {
        return this.http.delete(`${this.baseUrl}/api/bookcopy/${id}`);
    }
}
