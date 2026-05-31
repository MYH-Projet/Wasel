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
    LineChart,
    Settings,
    LogOut,
    Store
} from "lucide-react"

const mainNavItems = [
    { title: "Overview", icon: LayoutDashboard, url: "/admin", isActive: false },
    { title: "Requests", icon: Inbox, url: "/admin/requests", isActive: true },
    { title: "Complains", icon: MessageSquareWarning, url: "/admin/complains", isActive: false },
    { title: "Users", icon: Users, url: "/admin/users", isActive: false },
    { title: "Analytics", icon: LineChart, url: "/admin/analytics", isActive: false },
]

const bottomNavItems = [
    { title: "Settings", icon: Settings, url: "/admin/settings" },
    { title: "Logout", icon: LogOut, url: "/logout" },
]

export function AdminSidebar({ admin }: { admin: KeycloakPayload }) {
    const adminName = admin?.name || admin?.preferred_username || "System Admin";
    const initials = adminName.split(' ').map((n: string) => n[0]).join('').substring(0, 2).toUpperCase();
    const adminEmail = admin?.email || "admin@wasel.local";

    return (
        <Sidebar
            className="border-none"
            collapsible="icon"
            style={{
                '--sidebar': 'var(--secondary)',
                '--sidebar-foreground': 'var(--secondary-foreground)',
                '--sidebar-accent': 'rgba(255,255,255,0.1)',
                '--sidebar-accent-foreground': 'white',
            } as React.CSSProperties}
        >
            <SidebarHeader className="p-4 md:p-6 mb-2 group-data-[collapsible=icon]:p-2 group-data-[collapsible=icon]:mt-4">
                <div className="flex items-center gap-3 px-2 group-data-[collapsible=icon]:px-0 group-data-[collapsible=icon]:justify-center">
                    <div className="flex aspect-square size-10 group-data-[collapsible=icon]:size-8 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-sm font-bold text-lg group-data-[collapsible=icon]:text-sm">
                        {initials}
                    </div>
                    <div className="grid flex-1 text-left text-sm leading-tight group-data-[collapsible=icon]:hidden">
                        <span className="truncate font-bold text-[15px] tracking-wide text-white">{adminName}</span>
                        <span className="truncate text-xs text-slate-400 font-medium">{adminEmail}</span>
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
                                        isActive={item.isActive}
                                        tooltip={item.title}
                                        className={`transition-all duration-300 h-11 px-4 ${item.isActive
                                            ? "!bg-primary !text-primary-foreground font-bold shadow-md hover:!bg-primary/90 hover:!text-primary-foreground scale-[1.02]"
                                            : "text-slate-300 hover:text-white font-medium hover:scale-[1.02]"}`}
                                    >
                                        <a href={item.url}>
                                            <item.icon className={`size-5 mr-1 ${item.isActive ? 'text-primary-foreground' : 'text-slate-400 group-hover:text-white transition-colors'}`} />
                                            <span className="text-sm tracking-wide">{item.title}</span>
                                        </a>
                                    </SidebarMenuButton>
                                </SidebarMenuItem>
                            ))}
                        </SidebarMenu>
                    </SidebarGroupContent>
                </SidebarGroup>
            </SidebarContent>
            <SidebarFooter className="p-4 mb-2 group-data-[collapsible=icon]:p-0 group-data-[collapsible=icon]:mx-auto">
                <SidebarMenu className="gap-2">
                    {bottomNavItems.map((item) => (
                        <SidebarMenuItem key={item.title}>
                            <SidebarMenuButton
                                asChild
                                tooltip={item.title}
                                className="h-11 px-4 text-slate-300 hover:bg-white/10 hover:text-white font-medium transition-all duration-300 hover:scale-[1.02]"
                            >
                                <a href={item.url}>
                                    <item.icon className="size-5 mr-1 text-slate-400 group-hover:text-white transition-colors" />
                                    <span className="text-sm tracking-wide">{item.title}</span>
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