import {
    Dialog, DialogContent, DialogHeader, DialogTitle,
} from "@/components/ui/dialog";
import {
    Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from "@/components/ui/table";
import { ScrollArea } from "@/components/ui/scroll-area";
import {useEffect, useState} from "react";
import {api} from "../api/client.ts";
import {Button} from "../components/ui/button.tsx";

interface ShortLink {
    id: string;
    name: string | null;
    shortUrl: string;
    redirectUrl: string;
    description: string | null;
}

interface ViewAllModalProps {
    open: boolean;
    onClose: () => void;
    onDeleteRequest: (link: ShortLink) => void;
    onError: (msg:string) => void;
}

function ViewAllModal({ open, onClose, onDeleteRequest,onError }: ViewAllModalProps) {
    const [links, setLinks] = useState<ShortLink[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!open) return;
        setLoading(true);
        api.get<ShortLink[]>("/api/Link/user-links")
            .then(setLinks)
            .catch(err => onError((err as Error).message))
            .finally(() => setLoading(false));
    }, [open]);

    return (
        <Dialog open={open} onOpenChange={onClose}>
            <DialogContent className="max-w-0.5">
                <DialogHeader>
                    <DialogTitle><p className="text-black">My links</p></DialogTitle>
                </DialogHeader>

                <ScrollArea className="max-h-[60vh]">
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Label</TableHead>
                                <TableHead>Short URL</TableHead>
                                <TableHead>Destination</TableHead>
                                <TableHead></TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {loading ? (
                                <TableRow>
                                    <TableCell colSpan={4} className="text-center text-muted-foreground py-8">
                                        Loading...
                                    </TableCell>
                                </TableRow>
                            ) : links.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={4} className="text-center text-muted-foreground py-8">
                                        No links yet.
                                    </TableCell>
                                </TableRow>
                            ) :  links.map(link => (
                                    <TableRow key={link.id}>
                                        <TableCell className="font-medium">
                                            {link.name ?? <span className="text-muted-foreground">—</span>}
                                        </TableCell>
                                        <TableCell className="font-mono text-sm">{link.shortUrl}</TableCell>
                                        <TableCell className="max-w-[200px] truncate text-sm text-muted-foreground">
                                            {link.redirectUrl}
                                        </TableCell>
                                        <TableCell>
                                            <Button
                                                variant="destructive"
                                                size="sm"
                                                onClick={() => onDeleteRequest(link)}
                                            >
                                                Delete
                                            </Button>
                                        </TableCell>
                                    </TableRow>
                                ))}
                        </TableBody>
                    </Table>
                </ScrollArea>
            </DialogContent>
        </Dialog>
    );
}

export default ViewAllModal;