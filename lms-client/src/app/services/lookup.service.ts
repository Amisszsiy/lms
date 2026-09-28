import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { forkJoin, map, Observable, of, switchMap } from 'rxjs';
import { environment } from '../../environments/environment';
import { PagedResponse } from '../models/pagedResponse';
import { AuthorResponse, GenreResponse, LocationResponse, PublisherResponse } from '../models/lookups';

// Loads Author, Genre, Publisher and Location lists for dropdowns and for showing names in tables.
@Injectable({ providedIn: 'root' })
export class LookupService {
    private readonly baseUrl = environment.apiUrl;
    // Largest page size the API accepts.
    private readonly maxPageSize = 100;

    constructor(private http: HttpClient) {}

    getAuthors(): Observable<AuthorResponse[]> {
        return this.getAllPages<AuthorResponse>('author');
    }

    getGenres(): Observable<GenreResponse[]> {
        return this.getAllPages<GenreResponse>('genre');
    }

    getPublishers(): Observable<PublisherResponse[]> {
        return this.getAllPages<PublisherResponse>('publisher');
    }

    getLocations(): Observable<LocationResponse[]> {
        return this.getAllPages<LocationResponse>('location');
    }

    // Loads page 1, then every other page at the same time, and joins the items into one list.
    private getAllPages<T>(controller: string): Observable<T[]> {
        return this.getPage<T>(controller, 1).pipe(
            switchMap((firstPage: PagedResponse<T>) => {
                if (firstPage.totalPages <= 1) {
                    return of(firstPage.items);
                }

                const otherPageRequests: Observable<PagedResponse<T>>[] = [];
                for (let pageNo = 2; pageNo <= firstPage.totalPages; pageNo++) {
                    otherPageRequests.push(this.getPage<T>(controller, pageNo));
                }

                return forkJoin(otherPageRequests).pipe(
                    map((otherPages: PagedResponse<T>[]) => {
                        const allItems: T[] = [...firstPage.items];
                        for (const page of otherPages) {
                            allItems.push(...page.items);
                        }
                        return allItems;
                    })
                );
            })
        );
    }

    private getPage<T>(controller: string, pageNo: number): Observable<PagedResponse<T>> {
        let params = new HttpParams();
        params = params.set('pageNo', pageNo);
        params = params.set('pageSize', this.maxPageSize);

        return this.http.get<PagedResponse<T>>(`${this.baseUrl}/api/${controller}`, { params: params });
    }
}
