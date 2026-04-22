import { api } from "./client";

export interface TokenResponse {
    accessToken: string;
    refreshToken: string;
}

export const authApi = {
    login: (user: string, password: string) =>
        api.post<TokenResponse>("/Auth/Login", { user, password }),
    register: (username: string, full_Name: string, email: string, password: string) =>
        api.post<TokenResponse>("/Auth/Register", { username, full_Name, email, password }),
    logout: () => api.post<void>("/Auth/logout", {}),
};