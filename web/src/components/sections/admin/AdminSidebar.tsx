import {
    Sidebar,
    SidebarContent,
    SidebarFooter,
    SidebarGroup,
    SidebarGroupContent,
    SidebarHeader,
    SidebarMenu,
    SidebarMenuButton,
    SidebarMenuItem,
    SidebarRail
} from "@/components/ui/sidebar"
import type { KeycloakPayload } from "@/middleware"
import {
    LayoutDashboard,
    Inbox,
    MessageSquareWarning,
    Users,
    Car,
    Settings,
    LogOut,
    Map
} from "lucide-react"

// Updated icons and labels to match the image exactly
const mainNavItems = [
    { title: "Dashboard", icon: LayoutDashboard, nav_url: "/admin" },
    { title: "Map View", icon: Map, nav_url: "/admin/map" },
    { title: "Drivers", icon: Users, nav_url: "/admin/drivers" },
    { title: "Vehicles", icon: Car, nav_url: "/admin/vehicles" },
    { title: "Requests", icon: Inbox, nav_url: "/admin/requests" },
]

const bottomNavItems = [
    { title: "Settings", icon: Settings, url: "/admin/settings" },
    { title: "Logout", icon: LogOut, url: "/logout" },
]

export function AdminSidebar({ admin, url }: { admin: KeycloakPayload, url: string }) {
    const adminName = admin?.name || admin?.preferred_username || "Dispatcher Admin";
    const initials = adminName.split(' ').map((n: string) => n[0]).join('').substring(0, 2).toUpperCase();
    const adminEmail = admin?.email || "Super Admin";



    return (
        <Sidebar
            className="border-r border-sidebar-border"
            collapsible="icon"
            style={{
                '--sidebar': 'var(--sidebar)',
                '--sidebar-foreground': 'var(--sidebar-foreground)',
                '--sidebar-accent': 'var(--sidebar-accent)',
                '--sidebar-accent-foreground': 'var(--sidebar-accent-foreground)',
            } as React.CSSProperties}
        >
            <SidebarHeader className="p-4 md:p-6 mb-4 group-data-[collapsible=icon]:p-2 group-data-[collapsible=icon]:mt-4">
                <div className="flex items-center gap-3 px-2 group-data-[collapsible=icon]:px-0 group-data-[collapsible=icon]:justify-center">
                    <div className="flex aspect-square size-10 group-data-[collapsible=icon]:size-8 items-center justify-center rounded-full bg-primary text-primary-foreground shadow-sm font-bold text-sm group-data-[collapsible=icon]:text-xs">
                        {initials}
                    </div>
                    <div className="grid flex-1 text-left text-sm leading-tight group-data-[collapsible=icon]:hidden">
                        <span className="truncate font-bold text-[15px] tracking-wide text-sidebar-foreground">{adminName}</span>
                        <span className="truncate text-xs text-sidebar-foreground/70 font-medium">{adminEmail}</span>
                    </div>
                </div>
            </SidebarHeader>
            <SidebarContent className="px-3 group-data-[collapsible=icon]:px-0">
                <SidebarGroup>
                    <SidebarGroupContent>
                        <SidebarMenu className="gap-2">
                            {mainNavItems.map((item) => (
                                <SidebarMenuItem key={item.title}>
                                    <SidebarMenuButton
                                        asChild
                                        isActive={
                                            item.nav_url === "/admin" ? url === "/admin" : url.startsWith(item.nav_url)
                                        }
                                        tooltip={item.title}
                                        className={`transition-all duration-200 h-11 px-4 rounded-md ${(item.nav_url === "/admin" ? url === "/admin" : url.startsWith(item.nav_url))
                                            ? "bg-primary! text-primary-foreground font-bold hover:text-primary-foreground"
                                            : "text-sidebar-foreground/70 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground font-medium"
                                            }`}
                                    >
                                        <a href={item.nav_url}>
                                            <item.icon className={`size-5 mr-3 ${(item.nav_url === "/admin" ? url === "/admin" : url.startsWith(item.nav_url)) ? 'text-primary-foreground' : 'text-sidebar-foreground/60 group-hover:text-sidebar-accent-foreground transition-colors'}`} />
                                            <span className="text-[15px]">{item.title}</span>
                                        </a>
                                    </SidebarMenuButton>
                                </SidebarMenuItem>
                            ))}
                        </SidebarMenu>
                    </SidebarGroupContent>
                </SidebarGroup>
            </SidebarContent>
            <SidebarFooter className="p-4 mb-2 border-t border-sidebar-border group-data-[collapsible=icon]:p-0 group-data-[collapsible=icon]:border-none group-data-[collapsible=icon]:mx-auto">
                <SidebarMenu className="gap-2">
                    {bottomNavItems.map((item) => (
                        <SidebarMenuItem key={item.title}>
                            <SidebarMenuButton
                                asChild
                                tooltip={item.title}
                                className="h-11 px-4 text-sidebar-foreground/70 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground font-medium transition-all duration-200 rounded-md"
                            >
                                <a href={item.url}>
                                    <item.icon className="size-5 mr-3 text-sidebar-foreground/60 group-hover:text-sidebar-accent-foreground transition-colors" />
                                    <span className="text-[15px]">{item.title}</span>
                                </a>
                            </SidebarMenuButton>
                        </SidebarMenuItem>
                    ))}
                </SidebarMenu>
            </SidebarFooter>
            <SidebarRail />
        </Sidebar>
    )
}