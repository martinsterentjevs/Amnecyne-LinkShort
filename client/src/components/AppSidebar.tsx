import {
    Sidebar,
    SidebarContent,
    SidebarGroup,
    SidebarGroupLabel,
    SidebarMenu,
    SidebarMenuItem
} from './ui/sidebar.tsx';

function AppSidebar(){
    return (<Sidebar>
            <SidebarContent>
                <SidebarGroup>
                    <SidebarGroupLabel>Links</SidebarGroupLabel>
                    <SidebarMenu>
                        <SidebarMenuItem onClick={onViewAll}>My Links</SidebarMenuItem>
                        <SidebarMenuItem onClick={onCustomLink}>Create custom link</SidebarMenuItem>
                    </SidebarMenu>
                </SidebarGroup>
            </SidebarContent>
    </Sidebar>)
}
export {AppSidebar}