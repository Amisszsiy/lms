import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { UserResponse } from '../models/user';
import { PagedResponse } from '../models/pagedResponse';

@Injectable({ providedIn: 'root'})
export class UserService{
    private readonly baseUrl = environment.apiUrl;
    constructor(private http: HttpClient){}

    getUser(id: string){
        return this.http.get<UserResponse>(`${this.baseUrl}/api/user/${id}`);
    }

    // Admin only.
    searchUsers(search: string, isActive: boolean | null, pageNo: number, pageSize: number){
        let params = new HttpParams();
        params = params.set('search', search);
        params = params.set('pageNo', pageNo);
        params = params.set('pageSize', pageSize);
        if (isActive !== null) {
            params = params.set('isActive', isActive);
        }

        return this.http.get<PagedResponse<UserResponse>>(`${this.baseUrl}/api/user`, { params: params });
    }
}
