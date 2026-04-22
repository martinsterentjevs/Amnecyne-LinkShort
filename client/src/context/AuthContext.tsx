import { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from "react";
import { authApi } from "../api/auth";
import { setAccessToken } from "../api/client";

interface AuthContextValue {
    isAuthenticated: boolean;
    isLoading: boolean;
    username: string | null;
    login(user: string, password: string): Promise<void>;
    register(username: string, fullName: string, email: string, password: string): Promise<void>;
    logout(): Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function parseUsername(token: string): string | null {
    try {
        const payload = JSON.parse(atob(token.split(".")[1])) as Record<string, string>;
        return payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ?? payload["sub"] ?? null;
    } catch { return null; }
}

export function AuthProvider({ children }: { children: ReactNode }) {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [username, setUsername] = useState<string | null>(null);

    useEffect(() => {
        const rt = localStorage.getItem("refreshToken");
        if (!rt) { setIsLoading(false); return; }

        fetch("/Auth/refresh", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ refreshToken: rt }),
        })
            .then(r => { if (!r.ok) throw new Error(); return r.json() as Promise<{ accessToken: string; refreshToken: string }>; })
            .then(data => {
                setAccessToken(data.accessToken);
                localStorage.setItem("refreshToken", data.refreshToken);
                setUsername(parseUsername(data.accessToken));
                setIsAuthenticated(true);
            })
            .catch(() => localStorage.removeItem("refreshToken"))
            .finally(() => setIsLoading(false));
    }, []);

    const login = useCallback(async (user: string, password: string) => {
        const data = await authApi.login(user, password);
        setAccessToken(data.accessToken);
        localStorage.setItem("refreshToken", data.refreshToken);
        setUsername(parseUsername(data.accessToken));
        setIsAuthenticated(true);
    }, []);

    const register = useCallback(async (username: string, fullName: string, email: string, password: string) => {
        const data = await authApi.register(username, fullName, email, password);
        setAccessToken(data.accessToken);
        localStorage.setItem("refreshToken", data.refreshToken);
        setUsername(parseUsername(data.accessToken));
        setIsAuthenticated(true);
    }, []);

    const logout = useCallback(async () => {
        try { await authApi.logout(); } catch { /* best effort */ }
        setAccessToken(null);
        localStorage.removeItem("refreshToken");
        setIsAuthenticated(false);
        setUsername(null);
    }, []);

    return (
        <AuthContext.Provider value={{ isAuthenticated, isLoading, username, login, register, logout }}>
    {children}
    </AuthContext.Provider>
);
}

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
    return ctx;
}