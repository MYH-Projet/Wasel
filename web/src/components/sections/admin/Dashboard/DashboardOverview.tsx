import { useDashboardPolling } from "@/hooks/useDashboardPolling";
import { format } from "date-fns";
import { MetricCard } from "./MetricCard";
import { ControlPanel } from "./ControlPanel";
import { FleetStatus } from "./FleetStatus";
import { DeliveryVolumeChart } from "./DeliveryVolumeChart"
import { Truck, Users, FileCheck, Calendar } from "lucide-react";

export function DashboardOverview() {
    const { metrics, isInitialLoading, lastUpdated } = useDashboardPolling("/endpoint/dashboard/metrics", 30000);

    if (isInitialLoading || !metrics) {
        return (
            <div className="flex items-center justify-center h-[60vh]">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-yellow-500"></div>
            </div>
        );
    }

    const activeDiff = (metrics.activeDeliveries || 0) - (metrics.activeDeliveriesLastHour || 0);
    const activeTrendPercent = metrics.activeDeliveriesLastHour > 0 ? (activeDiff / metrics.activeDeliveriesLastHour) * 100 : (activeDiff > 0 ? 100 : 0);
    
    const signupsDiff = (metrics.newSignupsToday || 0) - (metrics.newSignupsYesterday || 0);
    const signupsTrendPercent = metrics.newSignupsYesterday > 0 ? (signupsDiff / metrics.newSignupsYesterday) * 100 : (signupsDiff > 0 ? 100 : 0);

    return (
        <div className="space-y-6">

            {/* Header & Polling Badge */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div>
                    <h1 className="text-3xl font-black tracking-tight text-slate-900">Dashboard Overview</h1>
                    <p className="text-slate-500 mt-1">High-level metrics and platform status.</p>
                </div>
                <div className="flex items-center gap-2 px-3 py-1.5 bg-green-50 text-green-700 text-xs font-bold rounded-full border border-green-200 shadow-sm">
                    <span className="w-2 h-2 rounded-full bg-green-500 animate-pulse"></span>
                    Auto-updating every 30s • {format(lastUpdated, "HH:mm")}
                </div>
            </div>

            {/* Top Row: Metric Cards Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6">

                <MetricCard
                    title="Active Deliveries"
                    value={metrics.activeDeliveries}
                    icon={Truck} iconBgColor="bg-yellow-50" iconTextColor="text-yellow-700"
                    trend={{ 
                        value: `${Math.abs(activeTrendPercent).toFixed(1)}%`, 
                        isPositive: activeDiff >= 0, 
                        label: "vs last hour" 
                    }}
                />

                <MetricCard
                    title="Today's Total"
                    value={metrics.todayTotal}
                    icon={Calendar} iconBgColor="bg-blue-50" iconTextColor="text-blue-700"
                    footer={
                        <div className="flex gap-4 text-xs font-medium">
                            <div className="text-green-600"><span className="block font-bold text-sm">{metrics.todayDone}</span> Done</div>
                            <div className="text-yellow-600"><span className="block font-bold text-sm">{metrics.activeDeliveries}</span> Active</div>
                            <div className="text-red-600"><span className="block font-bold text-sm">{metrics.todayCancelled}</span> Canc.</div>
                        </div>
                    }
                />

                <MetricCard
                    title="Available Drivers"
                    value={metrics.availableDrivers}
                    icon={Users} iconBgColor="bg-green-50" iconTextColor="text-green-700"
                    footer={<div className="text-xs text-slate-500 flex items-center gap-1"><span className="w-1.5 h-1.5 rounded-full bg-green-500"></span> Online and ready</div>}
                />

                <MetricCard
                    title="Pending Validations"
                    value={metrics.pendingValidations}
                    icon={FileCheck} iconBgColor="bg-orange-50" iconTextColor="text-orange-700"
                    footer={<span className="text-xs font-bold text-orange-600 bg-orange-50 px-2 py-1 rounded">Action required</span>}
                />
            </div>

            {/* Bottom Section: 3-Column Layout */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">

                {/* Left Area (Takes up 2/3 space) */}
                <div className="lg:col-span-2 space-y-6">
                    {/* Placeholder for future Chart component */}
                    <div className="bg-white rounded-xl shadow-sm border border-slate-200">
                        <DeliveryVolumeChart />
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <MetricCard
                            title="New Sign-ups (Today)"
                            value={metrics.newSignupsToday}
                            icon={Users} iconBgColor="bg-purple-50" iconTextColor="text-purple-700"
                            trend={{ 
                                value: `${Math.abs(signupsTrendPercent).toFixed(1)}%`, 
                                isPositive: signupsDiff >= 0, 
                                label: "vs yesterday" 
                            }}
                        />
                        
                        <FleetStatus 
                            onDelivery={metrics.fleetStatus?.onDelivery || 0} 
                            available={metrics.fleetStatus?.available || 0} 
                            inactive={metrics.fleetStatus?.inactive || 0} 
                        />
                    </div>
                </div>

                {/* Right Area: Control Panel (Takes up 1/3 space) */}
                <div className="lg:col-span-1">
                    <ControlPanel />
                </div>

            </div>
        </div>
    );
}