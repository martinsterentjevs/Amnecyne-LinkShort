import { Button } from "@/components/ui/button";
import { useAuth } from "../context/AuthContext";

interface HeaderProps {
    onLogin: () => void;
    onRegister: () => void;
}

function Header({ onLogin, onRegister }: HeaderProps) {
    const { isAuthenticated, username, logout } = useAuth();

    return (
        <header className="fixed top-0 left-0 right-0 z-40 h-14 bg-violet-700 border-b border-zinc-800 flex items-center justify-between px-6">
            <span className="text-sm font-semibold text-white">LinkShort</span>

            <div className="flex items-center gap-3">
                {isAuthenticated ? (
                    <>
                        <span className="text-xs text-white/70">{username}</span>
                        <Button variant="ghost" onClick={logout}>Sign out</Button>
                    </>
                ) : (
                    <>
                        <Button variant="outline" className="bg-gray-50" onClick={onLogin}>Sign in</Button>
                        <Button onClick={onRegister}>Register</Button>
                    </>
                )}
            </div>
        </header>
    );
}

export default Header;