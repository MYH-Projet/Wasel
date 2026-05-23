import { SidebarProvider, SidebarTrigger } from "@/components/ui/sidebar";
import { AdminSidebar } from "@/components/sections/admin/AdminSidebar";
import { useState } from "react";


export default function Sidebar({ children, admin, url }: { children: React.ReactNode, admin: KeycloakPayload, url: string }) {
    const [open, setOpen] = useState(false);
    return (
        <SidebarProvider open={open} onOpenChange={setOpen}>
            <AdminSidebar admin={admin} url={url} />
            <main className="w-full flex-1">
                {children}
            </main>
        </SidebarProvider>
    );
}