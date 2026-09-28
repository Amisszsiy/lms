import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { BookResponse, SaveBookRequest, SearchBooksRequest } from '../models/book';
import { PagedResponse } from '../models/pagedResponse';

@Injectable({ providedIn: 'root' })
export class BookService {
    private readonly baseUrl = environment.apiUrl;
    constructor(private http: HttpClient) {}

    searchBooks(request: SearchBooksRequest) {
        let params = new HttpParams();
        params = params.set('search', request.search);
        params = params.set('includeInactive', request.includeInactive);
        params = params.set('pageNo', request.pageNo);
        params = params.set('pageSize', request.pageSize);

        return this.http.get<PagedResponse<BookResponse>>(`${this.baseUrl}/api/book`, { params: params });
    }

    getBook(id: number) {
        return this.http.get<BookResponse>(`${this.baseUrl}/api/book/${id}`);
    }

    addBook(request: SaveBookRequest) {
        return this.http.post<BookResponse>(`${this.baseUrl}/api/book`, request);
    }

    updateBook(id: number, request: SaveBookRequest) {
        return this.http.put<BookResponse>(`${this.baseUrl}/api/book/${id}`, request);
    }

    // Soft delete: deactivates the book and withdraws its copies.
    deactivateBook(id: number) {
        return this.http.delete(`${this.baseUrl}/api/book/${id}`);
    }
}
