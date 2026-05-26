import type { APIRoute } from "astro";

export const GET: APIRoute = async (context) => {
    try {
        // ⏳ Simulate network delay for the loading state (e.g. spinner)
        await new Promise((resolve) => setTimeout(resolve, 600));

        // ✅ Return mock metrics for the admin dashboard
        const mockMetrics = {
            activeDeliveries: 12,
            todayTotal: 85,
            todayDone: 64,
            todayCancelled: 9,
            availableDrivers: 28,
            pendingValidations: 5,
            newSignups: 15
        };

        return new Response(JSON.stringify(mockMetrics), {
            status: 200,
            headers: { 
                "Content-Type": "application/json",
                "Cache-Control": "no-store, max-age=0"
            },
        });
    } catch (error) {
        console.error("Mock Dashboard Metrics Endpoint Error:", error);
        return new Response(JSON.stringify({ message: "Internal server error" }), {
            status: 500,
            headers: { "Content-Type": "application/json" },
        });
    }
};
