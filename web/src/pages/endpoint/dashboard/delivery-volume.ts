import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

export const GET: APIRoute = async (context) => {
    try {
        const user = context.locals.user;
        const url = new URL(context.request.url);
        const timeFrame = url.searchParams.get("timeFrame") || "12h";

        // Map frontend timeFrame to backend range
        let range = "today";
        switch (timeFrame) {
            case "12h":
            case "24h":
                range = "today";
                break;
            case "7d":
                range = "week";
                break;
            case "1m":
                range = "month";
                break;
            case "1y":
                range = "year";
                break;
        }

        const response = await fetch(`${INTERNAL_API_URL}/api/admin/dashboard/delivery-volume?range=${range}`, {
            method: "GET",
            headers: {
                Authorization: `Bearer ${user?.token}`,
            },
        });

        if (!response.ok) {
            const isJson = response.headers.get("content-type")?.includes("application/json");
            const errorMessage = isJson
                ? (await response.json()).message || "Backend error"
                : response.statusText;

            return new Response(JSON.stringify({ message: errorMessage }), {
                status: response.status,
                headers: { "Content-Type": "application/json" },
            });
        }

        const data = await response.json();

        return new Response(JSON.stringify(data), {
            status: 200,
            headers: {
                "Content-Type": "application/json",
                "Cache-Control": "no-store, max-age=0"
            },
        });
    } catch (error) {
        console.error("Delivery Volume Endpoint Error:", error);
        return new Response(JSON.stringify({ message: "Internal server error" }), {
            status: 500,
            headers: { "Content-Type": "application/json" },
        });
    }
};