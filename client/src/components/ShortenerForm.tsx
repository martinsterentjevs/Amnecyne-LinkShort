import { useState } from "react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useAuth } from "../context/AuthContext";
import { api } from "../api/client";

interface ShortLink {
    id: string;
    shortUrl: string;
    redirectUrl: string;
}
interface ShortenerFormProps {
    onError: (msg: string) => void;
}

function ShortenerForm({ onError }: ShortenerFormProps) {
    const { isAuthenticated } = useAuth();
    const SHORT_BASE = import.meta.env.VITE_SHORT_BASE_URL ?? window.location.origin;

    const [createUrl, setCreateUrl] = useState("");
    const [createResult, setCreateResult] = useState<string | null>(null);
    const [createLoading, setCreateLoading] = useState(false);
    const [createCopied, setCreateCopied] = useState(false);

    const [resolveUrl, setResolveUrl] = useState("");
    const [resolveResult, setResolveResult] = useState<string | null>(null);
    const [resolveLoading, setResolveLoading] = useState(false);

    async function handleCreate(e: React.FormEvent) {
        e.preventDefault();
        setCreateLoading(true);
        setCreateResult(null);
        setResolveResult(null);
        try {
            const endpoint = isAuthenticated ? "/api/Link/create" : "/api/Link/create-random";
            const link = await api.post<ShortLink>(endpoint, { redirectUrl: createUrl });
            setCreateResult(`${SHORT_BASE}/${link.shortUrl}`);
            setCreateUrl("");
        } catch (err) {
            onError((err as Error).message);
        } finally {
            setCreateLoading(false);
        }
    }

    async function handleResolve(e: React.FormEvent) {
        e.preventDefault();
        setResolveLoading(true);
        setResolveResult(null);
        setCreateResult(null);
        try {
            const code = resolveUrl.trim().split("/").pop() ?? resolveUrl.trim();
            const link = await api.get<ShortLink>(`/api/Link/resolve/${code}`);
            setResolveResult(link.redirectUrl)
            setResolveUrl("");
        } catch (err) {
            onError((err as Error).message);
        } finally {
            setResolveLoading(false);
        }
    }

    function handleCopy() {
        if (!createResult) return;
        navigator.clipboard.writeText(createResult);
        setCreateCopied(true);
        setTimeout(() => setCreateCopied(false), 2000);
    }

    const hasResult = createResult ?? resolveResult;

    return (
        <div className="w-full max-w-5xl flex flex-col gap-4 px-8">
            {/* Top row — two cards side by side */}
            <div className="grid grid-cols-2 gap-4">
                <Card className="flex flex-col">
                    <CardHeader className="pb-2">
                        <CardTitle className="text-base text-center font-semibold">Create short link</CardTitle>
                    </CardHeader>
                    <CardContent className="flex flex-col gap-3 flex-1 pt-4">
                        <form onSubmit={handleCreate} className="flex flex-col gap-3">
                            <Input
                                type="url"
                                placeholder="Input destination link"
                                value={createUrl}
                                onChange={e => setCreateUrl(e.target.value)}
                                required
                            />
                            <Button type="submit" disabled={createLoading} className="w-full">
                                {createLoading ? "Shortening..." : "Shorten"}
                            </Button>
                        </form>
                    </CardContent>
                </Card>

                <Card className="flex flex-col">
                    <CardHeader className="pb-2">
                        <CardTitle className="text-base">Unravel short link</CardTitle>
                    </CardHeader>
                    <CardContent className="flex flex-col gap-3 flex-1 pt-4">
                        <form onSubmit={handleResolve} className="flex flex-col gap-3">
                            <Input
                                type="text"
                                placeholder="Input shortened link"
                                value={resolveUrl}
                                onChange={e => setResolveUrl(e.target.value)}
                                required
                            />
                            <Button type="submit" disabled={resolveLoading} className="w-full">
                                {resolveLoading ? "Resolving..." : "Unravel"}
                            </Button>
                        </form>
                    </CardContent>
                </Card>
            </div>

            {/* Result card — full width, only shows when there's a result */}
            {hasResult && (
                <Card>
                    <CardHeader>
                        <CardTitle className="text-base">Result</CardTitle>
                    </CardHeader>
                    <CardContent className="flex items-center justify-between gap-4">
                        <span className="text-sm text-muted-foreground">The URL requested is:</span>
                        {createResult && (
                            <div className="flex items-center gap-2 flex-1">

                            <a    href={createResult}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="font-mono text-sm truncate hover:underline flex-1"
                                >
                                {createResult}
                            </a>
                            <Button variant="ghost" size="sm" onClick={handleCopy}>
                        {createCopied ? "Copied!" : "Copy"}
                    </Button>
                </div>
                )}
            {resolveResult && (

              <a  href={resolveResult}
                target="_blank"
                rel="noopener noreferrer"
                className="font-mono text-sm truncate hover:underline flex-1"
                >
            {resolveResult}
                </a>
                )}
        </CardContent>
</Card>
)}
</div>
);
}

export default ShortenerForm;