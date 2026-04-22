import {
    AlertDialog, AlertDialogContent, AlertDialogHeader, AlertDialogTitle,
    AlertDialogDescription, AlertDialogFooter, AlertDialogCancel, AlertDialogAction,
} from "@/components/ui/alert-dialog";

interface ShortLink {
    id: string;
    name: string | null;
    shortUrl: string;
    redirectUrl: string;
    description: string | null;
}

interface DeleteModalProps {
    open: boolean;
    onClose: () => void;
    onConfirm: () => void;
    link: ShortLink | null;
}

function DeleteModal({ open, onClose, onConfirm, link }: DeleteModalProps) {
    if (!link) return null;

    return (
        <AlertDialog open={open} onOpenChange={onClose}>
            <AlertDialogContent>
                <AlertDialogHeader>
                    <AlertDialogTitle>Delete link</AlertDialogTitle>
                    <AlertDialogDescription>
                        Are you sure you want to delete{" "}
                        <span className="font-mono font-medium text-foreground">{link.shortUrl}</span>
                        {link.name && <> "{link.name}"</>}? This cannot be undone.
                    </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                    <AlertDialogCancel onClick={onClose}>Cancel</AlertDialogCancel>
                    <AlertDialogAction onClick={onConfirm} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
                        Delete
                    </AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
    );
}

export default DeleteModal;