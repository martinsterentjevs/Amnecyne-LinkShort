import {
    Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { useState } from "react";
import {useAuth} from "../context/AuthContext.tsx";

interface LoginModalProps {
    open: boolean;
    onClose: () => void;
    onSwitchToRegister: () => void;
    onError: (msg: string) => void;
}

function LoginModal({ open, onClose, onSwitchToRegister, onError }: LoginModalProps) {
    const {login} = useAuth();
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [loading, setLoading] = useState(false);
    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setLoading(true);
        try {
            await login(username, password);
            onClose();
        } catch (err) {
            onError((err as Error).message);
        } finally {
            setLoading(false);
        }
    }

    return (
        <Dialog open={open} onOpenChange={onClose}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle><p className="text-black">Sign in</p></DialogTitle>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    <div className="flex flex-col gap-2">
                        <Label htmlFor="username">Username</Label>
                        <Input id="username" value={username} onChange={e => setUsername(e.target.value)} required />
                    </div>
                    <div className="flex flex-col gap-2">
                        <Label htmlFor="password">Password</Label>
                        <Input id="password" type="password" value={password} onChange={e => setPassword(e.target.value)} required />
                    </div>
                    <Button type="submit" className="w-full" disabled={loading}>{loading?"Signing in...":"Sign in"}</Button>
                    <DialogFooter className="flex-col gap-0">

                        <p className="text-center text-xs text-muted-foreground">
                            No account?{" "}
                            <button type="button" disabled={loading} onClick={onSwitchToRegister} className="text-primary hover:underline">
                                Register
                            </button>
                        </p>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}

export default LoginModal;