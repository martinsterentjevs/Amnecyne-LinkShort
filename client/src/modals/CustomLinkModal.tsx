import {
    Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from '@/components/ui/dialog';
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { useState } from "react";
import {api} from "../api/client.ts";

interface CustomLinkModalProps {
    open: boolean;
    onClose: () => void;
    onError: (msg: string) => void;
}
interface ShortLinkResponse {
    id: string;
    shortUrl: string;
    redirectUrl: string;
}

function CustomLinkModal({ open, onClose, onError }: CustomLinkModalProps) {
    const [redirectUrl, setRedirectUrl] = useState("");
    const [slug, setSlug] = useState("");
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [result, setResult] = useState<string | null>(null);
    const [copied, setCopied] = useState(false);
    const [loading, setLoading] = useState(false);
    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setLoading(true);
        try {
            const link = await api.post<ShortLinkResponse>("/api/Link/create", {
                redirectUrl,
                customShortLink: slug || undefined,
                name: name || undefined,
                description: description || undefined,
            });
            const SHORT_BASE = import.meta.env.VITE_SHORT_BASE_URL ?? window.location.origin;
            setResult(`${SHORT_BASE}/${link.shortUrl}`);
        } catch (err) {
            onError((err as Error).message);
        } finally {
            setLoading(false);
        }
    }

    function handleCopy() {
        if (!result) return;
        navigator.clipboard.writeText(result);
        setCopied(true);
        setTimeout(() => setCopied(false), 2000);
    }

    function handleClose() {
        setResult(null);
        setCopied(false);
        setRedirectUrl("");
        setSlug("");
        setName("");
        setDescription("");
        onClose();
    }

    return (
        <Dialog open={open} onOpenChange={handleClose}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle><p className={"text-black"}>Create custom link</p></DialogTitle>
                </DialogHeader>

                {result ? (
                    <div className="flex flex-col gap-4">
                        <p className="text-sm text-muted-foreground">Link created successfully.</p>
                        <div className="flex items-center gap-2 rounded-lg border px-3 py-2">
                            <span className="font-mono text-sm flex-1 truncate">{result}</span>
                            <Button variant="ghost" size="sm" onClick={handleCopy}>
                                {copied ? "Copied!" : "Copy"}
                            </Button>
                        </div>
                        <DialogFooter>
                            <Button variant="outline" onClick={handleClose} className="w-full">Close</Button>
                        </DialogFooter>
                    </div>
                ) : (
                    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                        <div className="flex flex-col gap-2">
                            <Label htmlFor="redirectUrl">Destination URL</Label>
                            <Input id="redirectUrl" type="url" placeholder="https://example.com" value={redirectUrl} onChange={e => setRedirectUrl(e.target.value)} required />
                        </div>
                        <div className="flex flex-col gap-2">
                            <Label htmlFor="slug">Custom slug <span className="text-muted-foreground">(optional)</span></Label>
                            <Input id="slug" placeholder="e.g. download-android" value={slug} onChange={e => setSlug(e.target.value)} />
                        </div>
                        <div className="flex flex-col gap-2">
                            <Label htmlFor="name">Label <span className="text-muted-foreground">(optional)</span></Label>
                            <Input id="name" placeholder="e.g. Android App" value={name} onChange={e => setName(e.target.value)} />
                        </div>
                        <div className="flex flex-col gap-2">
                            <Label htmlFor="description">Description <span className="text-muted-foreground">(optional)</span></Label>
                            <Input id="description" value={description} onChange={e => setDescription(e.target.value)} />
                        </div>

                        <DialogFooter>
                            <Button type="submit" className="w-full" disabled={loading}>{loading ? "Creating link...":"Create link"}</Button>
                        </DialogFooter>
                    </form>
                )}
            </DialogContent>
        </Dialog>
    );
}

export default CustomLinkModal;