import {
    Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { useState } from "react";
import {useAuth} from "../context/AuthContext.tsx";

interface RegisterModalProps {
    open: boolean;
    onClose: () => void;
    onSwitchToLogin: () => void;
    onError: (msg: string) => void;
}

function RegisterModal({open, onClose, onSwitchToLogin,onError}: RegisterModalProps) {
    const { register } = useAuth();
    const [loading, setLoading] = useState(false);
    const [fullName, setFullName] = useState("");
    const [email, setEmail] = useState("");
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setLoading(true);
        try {
            await register(username, fullName, email, password);
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
                    <DialogTitle><p className="text-black">Sign up</p></DialogTitle>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    <div className="flex flex-col gap-2">
                        <Label htmlFor="fullName">Full name</Label>
                        <Input id="fullName" value={fullName} onChange={e => setFullName(e.target.value)} required />
                    </div>
                    <div className="flex flex-col gap-2">
                        <Label htmlFor="username">Username</Label>
                        <Input id="username" value={username} onChange={e => setUsername(e.target.value)} required />
                    </div>
                    <div className="flex flex-col gap-2">
                        <Label htmlFor="email">Email</Label>
                        <Input id="email" type="email" value={email} onChange={e => setEmail(e.target.value)} required />
                    </div>
                    <div className="flex flex-col gap-2">
                        <Label htmlFor="password">Password</Label>
                        <Input id="password" type="password" value={password} onChange={e => setPassword(e.target.value)} required />
                    </div>
                    <Button type="submit" className="w-full" disabled={loading}>{loading?"Signing up":"Sign up"}</Button>
                    <DialogFooter className="flex-col gap-2">

                        <p className="text-center text-xs text-muted-foreground">
                            Have an account?{" "}
                            <button type="button" disabled={loading} onClick={onSwitchToLogin} className="text-primary hover:underline" >
                                Sign in
                            </button>
                        </p>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}

export default RegisterModal;