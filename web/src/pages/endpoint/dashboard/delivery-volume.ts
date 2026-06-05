import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

interface DeliveryItem {
    id: string;
    createdAt: string;
    status: string;
}

export const GET: APIRoute = async (context) => {
    try {
        const user = context.locals.user;
        const token = user?.token;

        if (!token) {
            return new Response(JSON.stringify({ message: "Unauthorized" }), { status: 401 });
        }

        const url = new URL(context.request.url);
        const timeFrame = url.searchParams.get("timeFrame") || "12h";

        // Fetch all deliveries (up to 1000 for stats)
        const response = await fetch(`${INTERNAL_API_URL}/api/admin/deliveries?pageSize=1000`, {
            headers: {
                Authorization: `Bearer ${token}`,
            },
        });

        if (!response.ok) {
            return new Response(JSON.stringify({ message: "Failed to fetch deliveries from backend" }), {
                status: response.status,
                headers: { "Content-Type": "application/json" },
            });
        }

        const data = await response.json();
        const deliveries: DeliveryItem[] = data.items || [];

        const now = new Date();
        const result: { hour?: string; day?: string; month?: string; volume: number }[] = [];

        if (timeFrame === "12h" || timeFrame === "24h") {
            const hoursCount = timeFrame === "12h" ? 12 : 24;
            // Generate bins for the last N hours
            for (let i = hoursCount - 1; i >= 0; i--) {
                const d = new Date(now.getTime() - i * 60 * 60 * 1000);
                const hourLabel = `${String(d.getHours()).padStart(2, "0")}:00`;
                
                // Count deliveries in this hour slot
                const volume = deliveries.filter(item => {
                    const itemDate = new Date(item.createdAt);
                    return itemDate.getFullYear() === d.getFullYear() &&
                           itemDate.getMonth() === d.getMonth() &&
                           itemDate.getDate() === d.getDate() &&
                           itemDate.getHours() === d.getHours();
                }).length;

                result.push({ hour: hourLabel, volume });
            }
        } 
        else if (timeFrame === "7d") {
            const daysOfWeek = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
            for (let i = 6; i >= 0; i--) {
                const d = new Date(now.getFullYear(), now.getMonth(), now.getDate() - i);
                const dayLabel = daysOfWeek[d.getDay()];

                const volume = deliveries.filter(item => {
                    const itemDate = new Date(item.createdAt);
                    return itemDate.getFullYear() === d.getFullYear() &&
                           itemDate.getMonth() === d.getMonth() &&
                           itemDate.getDate() === d.getDate();
                }).length;

                result.push({ day: dayLabel, volume });
            }
        } 
        else if (timeFrame === "1m") {
            for (let i = 29; i >= 0; i--) {
                const d = new Date(now.getFullYear(), now.getMonth(), now.getDate() - i);
                const dayLabel = `${String(d.getMonth() + 1).padStart(2, "0")}/${String(d.getDate()).padStart(2, "0")}`;

                const volume = deliveries.filter(item => {
                    const itemDate = new Date(item.createdAt);
                    return itemDate.getFullYear() === d.getFullYear() &&
                           itemDate.getMonth() === d.getMonth() &&
                           itemDate.getDate() === d.getDate();
                }).length;

                result.push({ day: dayLabel, volume });
            }
        } 
        else if (timeFrame === "1y") {
            const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
            for (let i = 11; i >= 0; i--) {
                const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
                const monthLabel = months[d.getMonth()];

                const volume = deliveries.filter(item => {
                    const itemDate = new Date(item.createdAt);
                    return itemDate.getFullYear() === d.getFullYear() &&
                           itemDate.getMonth() === d.getMonth();
                }).length;

                result.push({ month: monthLabel, volume });
            }
        }

        return new Response(JSON.stringify(result), {
            status: 200,
            headers: {
                "Content-Type": "application/json",
                "Cache-Control": "no-store, max-age=0"
            },
        });

    } catch (error) {
        console.error("Dashboard Delivery Volume Endpoint Error:", error);
        return new Response(JSON.stringify({ message: "Internal server error" }), {
            status: 500,
            headers: { "Content-Type": "application/json" },
        });
    }
};