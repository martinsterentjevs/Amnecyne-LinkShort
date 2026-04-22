const BASE_URL = import.meta.env.VITE_API_URL ?? "";

let accessToken: string | null = null;

export function setAccessToken(t: string | null) { accessToken = t; }
export function getAccessToken() { return accessToken; }

async function tryRefresh(): Promise<boolean> {
    const rt = localStorage.getItem("refreshToken");
    if (!rt) return false;
    const res = await fetch(`${BASE_URL}/Auth/refresh`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken: rt }),
    });
    if (!res.ok) {
        localStorage.removeItem("refreshToken");
        setAccessToken(null);
        return false;
    }
    const data = await res.json() as { accessToken: string; refreshToken: string };
    setAccessToken(data.accessToken);
    localStorage.setItem("refreshToken", data.refreshToken);
    return true;
}

export async function apiFetch<T>(path: string, options: RequestInit = {}, retry = true): Promise<T> {
    const headers: Record<string, string> = {
        "Content-Type": "application/json",
        ...(options.headers as Record<string, string>),
    };
    if (accessToken) headers["Authorization"] = `Bearer ${accessToken}`;

    const res = await fetch(`${BASE_URL}${path}`, { ...options, headers });

    if (res.status === 401 && retry) {
        const ok = await tryRefresh();
        if (ok) return apiFetch<T>(path, options, false);
        throw new Error("Unauthorized");
    }
    if (!res.ok) {
        const err = await res.json().catch(() => ({ message: res.statusText })) as { message?: string };
        throw new Error(err.message ?? "Request failed");
    }
    if (res.status === 204) return undefined as T;
    return await res.json() as Promise<T>;
}

export const api = {
    get: <T,>(p: string) => apiFetch<T>(p),
    post: <T,>(p: string, b: unknown) => apiFetch<T>(p, { method: "POST", body: JSON.stringify(b) }),
    del: <T,>(p: string) => apiFetch<T>(p, { method: "DELETE" }),
};