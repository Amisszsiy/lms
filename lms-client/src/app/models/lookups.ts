// Read-only data used to fill dropdowns and to show names instead of ids.

export interface AuthorResponse {
    id: number;
    firstName: string;
    lastName: string;
    penNames: string[];
}

export interface GenreResponse {
    id: number;
    genreName: string;
}

export interface PublisherResponse {
    id: number;
    name: string;
    tel: string;
}

export interface LocationResponse {
    id: number;
    area: string;
    lineNo: string;
    shelfNo: string;
    rowNo: number;
    columnNo: number;
}

// e.g. "A-1-2 R3 C1" = Area A, Line 1, Shelf 2, Row 3, Column 1.
export function getLocationLabel(location: LocationResponse): string {
    return `${location.area}-${location.lineNo}-${location.shelfNo} R${location.rowNo} C${location.columnNo}`;
}
