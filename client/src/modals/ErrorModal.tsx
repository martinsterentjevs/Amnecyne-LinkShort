import {
    AlertDialog, AlertDialogContent, AlertDialogHeader, AlertDialogTitle,
    AlertDialogDescription, AlertDialogFooter, AlertDialogAction,
} from "@/components/ui/alert-dialog";

interface ErrorModalProps {
    open: boolean;
    message: string | null;
    onClose: () => void;
}

function ErrorModal({ open, message, onClose }: ErrorModalProps) {
    return (
        <AlertDialog open={open} onOpenChange={onClose}>
            <AlertDialogContent>
                <AlertDialogHeader>
                    <AlertDialogTitle>Something went wrong</AlertDialogTitle>
                    <AlertDialogDescription>
                        {message ?? "An unexpected error occurred."}
                    </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                    <AlertDialogAction onClick={onClose}>Dismiss</AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
    );
}

export default ErrorModal;