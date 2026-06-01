import { Inbox, Truck, UserX, Settings, Map, MessageSquareWarning } from "lucide-react";
import {
    Card,
    CardHeader,
    CardTitle,
    CardDescription,
    CardContent,
    CardFooter
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export function ControlPanel() {
    return (
        <Card className="flex flex-col h-full shadow-sm border-none">
            <CardHeader>
                <CardTitle className="text-lg font-bold text-slate-900 flex items-center gap-2">
                    <span className="text-yellow-500">⚡</span> Control Panel
                </CardTitle>
                <CardDescription>Quick operational actions</CardDescription>
            </CardHeader>

            <CardContent className="flex-grow space-y-1">
                {/* 1. Pending Validations */}
                <a href="/admin/requests" className="flex items-center gap-4 p-3 rounded-lg hover:bg-slate-50 transition-colors group">
                    <div className="p-2 bg-yellow-50 text-yellow-700 rounded-md group-hover:scale-105 transition-transform">
                        <Inbox className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-bold text-slate-900 text-sm">Pending Validations</p>
                        <p className="text-xs text-slate-500">Review driver dossiers</p>
                    </div>
                </a>

                {/* 2. Active Deliveries */}
                <a href="/admin/deliveries" className="flex items-center gap-4 p-3 rounded-lg hover:bg-slate-50 transition-colors group">
                    <div className="p-2 bg-blue-50 text-blue-700 rounded-md group-hover:scale-105 transition-transform">
                        <Truck className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-bold text-slate-900 text-sm">Active Deliveries</p>
                        <p className="text-xs text-slate-500">Monitor live transit</p>
                    </div>
                </a>

                {/* 3. Live Map */}
                <a href="/admin/map" className="flex items-center gap-4 p-3 rounded-lg hover:bg-slate-50 transition-colors group">
                    <div className="p-2 bg-green-50 text-green-700 rounded-md group-hover:scale-105 transition-transform">
                        <Map className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-bold text-slate-900 text-sm">Live Fleet Map</p>
                        <p className="text-xs text-slate-500">Track drivers in real-time</p>
                    </div>
                </a>

                {/* 4. Complaints */}
                <a href="/admin/complains" className="flex items-center gap-4 p-3 rounded-lg hover:bg-slate-50 transition-colors group">
                    <div className="p-2 bg-orange-50 text-orange-700 rounded-md group-hover:scale-105 transition-transform">
                        <MessageSquareWarning className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-bold text-slate-900 text-sm">User Complaints</p>
                        <p className="text-xs text-slate-500">Review unresolved tickets</p>
                    </div>
                </a>

                {/* 5. Blocked Users */}
                <a href="/admin/users?status=BLOCKED" className="flex items-center gap-4 p-3 rounded-lg hover:bg-slate-50 transition-colors group">
                    <div className="p-2 bg-red-50 text-red-700 rounded-md group-hover:scale-105 transition-transform">
                        <UserX className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-bold text-slate-900 text-sm">Blocked Users</p>
                        <p className="text-xs text-slate-500">Manage restrictions</p>
                    </div>
                </a>
            </CardContent>

            <CardFooter>
                <Button variant="outline" className="w-full gap-2 text-slate-700 font-bold" asChild>
                    <a href="/admin/settings">
                        <Settings className="w-4 h-4" /> View All Settings
                    </a>
                </Button>
            </CardFooter>
        </Card>
    );
}