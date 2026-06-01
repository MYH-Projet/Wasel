import { useState, useEffect } from "react";
import { toast } from "sonner";

interface DashboardMetrics {
    activeDeliveries: number;
    todayTotal: number;
    todayDone: number;
    todayCancelled: number;
    availableDrivers: number;
    pendingValidations: number;
    newSignups: number;
}



export function useDashboardPolling(endpoint: string, pollingIntervalMs = 30000) {
    const [metrics, setMetrics] = useState<DashboardMetrics | null>(null);
    const [isInitialLoading, setIsInitialLoading] = useState(true);
    const [lastUpdated, setLastUpdated] = useState<Date>(new Date());

    useEffect(() => {
        // The fetch function
        const fetchMetrics = async () => {
            try {
                const response = await fetch(endpoint);
                const data = await response.json();
                if (!response.ok) {
                    throw new Error(data.message || `Dashboard data is not available right now. Please try again later`);
                }
                setMetrics(data);
                setLastUpdated(new Date());

            } catch (error) {
                console.error("Dashboard polling failed", error);
            } finally {
                setIsInitialLoading(false);
            }
        };

        fetchMetrics();

        const intervalId = setInterval(() => {
            fetchMetrics();
        }, pollingIntervalMs);
        return () => clearInterval(intervalId);

    }, [endpoint, pollingIntervalMs]);

    return { metrics, isInitialLoading, lastUpdated };
}