interface AppSidebarProps {
    onViewAll: () => void;
    onCustomLink: () => void;
}

export function AppSidebar({ onViewAll, onCustomLink }: AppSidebarProps) {
    return (
        <aside className="fixed top-14 left-0 h-[calc(100vh-3.5rem)] w-56 border-r bg-sidebar flex flex-col px-3 py-4 gap-1">
            <p className="text-xs text-muted-foreground uppercase tracking-widest px-2 mb-2">Links</p>
            <button onClick={onViewAll} className="text-sm text-left px-3 py-2 rounded-lg hover:bg-sidebar-accent hover:text-sidebar-accent-foreground transition-colors">
                My Links
            </button>
            <button onClick={onCustomLink} className="text-sm text-left px-3 py-2 rounded-lg hover:bg-sidebar-accent hover:text-sidebar-accent-foreground transition-colors">
                Create custom link
            </button>
        </aside>
    );
}