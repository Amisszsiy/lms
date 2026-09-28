import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { AssignBorrowRequest, BorrowingResponse, BorrowRequest, SearchBorrowingsRequest } from '../models/borrowing';
import { PagedResponse } from '../models/pagedResponse';

@Injectable({ providedIn: 'root' })
export class BorrowingService {
    private readonly baseUrl = environment.apiUrl;
    constructor(private http: HttpClient) {}

    // Admin: every borrowing.
    searchBorrowings(request: SearchBorrowingsRequest) {
        const params = this.toParams(request);
        return this.http.get<PagedResponse<BorrowingResponse>>(`${this.baseUrl}/api/borrow`, { params: params });
    }

    // Current user's own borrowings.
    searchMyBorrowings(request: SearchBorrowingsRequest) {
        const params = this.toParams(request);
        return this.http.get<PagedResponse<BorrowingResponse>>(`${this.baseUrl}/api/borrow/me`, { params: params });
    }

    // Current user borrows a copy.
    borrow(request: BorrowRequest) {
        return this.http.post<BorrowingResponse>(`${this.baseUrl}/api/borrow`, request);
    }

    // Admin lends a copy to a chosen user.
    assign(request: AssignBorrowRequest) {
        return this.http.post<BorrowingResponse>(`${this.baseUrl}/api/borrow/assign`, request);
    }

    // Current user asks to return: Borrowed -> ReturnRequested.
    requestReturn(id: string) {
        return this.http.post<BorrowingResponse>(`${this.baseUrl}/api/borrow/${id}/return-request`, {});
    }

    // Admin closes the loan and puts the copy back to Available.
    processReturn(id: string) {
        return this.http.post<BorrowingResponse>(`${this.baseUrl}/api/borrow/${id}/return`, {});
    }

    private toParams(request: SearchBorrowingsRequest): HttpParams {
        let params = new HttpParams();
        params = params.set('search', request.search);
        params = params.set('pageNo', request.pageNo);
        params = params.set('pageSize', request.pageSize);
        if (request.status !== null) {
            params = params.set('status', request.status);
        }
        return params;
    }
}
