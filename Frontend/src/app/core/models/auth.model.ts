export interface LoginResponse {
    accessToken: string;
    refreshToken: string;
    accessTokenExpiry: string;
    userId: string;
    email: string;
    role: string;
    employeeId: string;
    fullName: string;
}

export interface User {
    id: string;
    email: string;
    role: string;
    fullName: string;
    employeeId?: string;
}
