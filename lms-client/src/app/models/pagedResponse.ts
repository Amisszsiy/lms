export interface PagedResponse<T> {
    items: T[];
    pageNo: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}
