export interface PaginationMeta {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export interface ApiResponse<T> {
    success: boolean;
    message: string;
    errors: string[];
    data: T;
    pagination?: PaginationMeta;
}

export interface PagedResponse<T> {
    success: boolean;
    message: string;
    data: T[];
    pagination: PaginationMeta;
}
