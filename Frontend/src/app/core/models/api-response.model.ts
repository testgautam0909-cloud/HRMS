export interface ApiResponse<T> {
    succeeded: boolean;
    message: string;
    errors: string[];
    data: T;
}

export interface PaginatedResponse<T> {
    pageNumber: number;
    pageSize: number;
    totalPages: number;
    totalRecords: number;
    data: T[];
}
