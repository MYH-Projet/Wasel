import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

export const GET: APIRoute = async (context) => {

    const user = context.locals.user;
    try {
        // 1. Parse and map the query parameters sent by the React frontend
        const url = new URL(context.request.url);
        const searchParams = url.searchParams;

        const backendParams = new URLSearchParams();
        if (searchParams.has("page")) backendParams.set("page", searchParams.get("page")!);
        if (searchParams.has("pageSize")) backendParams.set("pageSize", searchParams.get("pageSize")!);
        if (searchParams.has("search")) backendParams.set("search", searchParams.get("search")!);

        const statusFilter = searchParams.get("statusFilter");
        if (statusFilter) {
            // Replace underscores (e.g. PENDING_VERIFICATION -> PENDINGVERIFICATION)
            const cleanStatus = statusFilter.replace(/_/g, "");
            backendParams.set("statusFilter", cleanStatus);
        }

        // 2. Call the .NET backend API with the mapped query parameters
        const response = await fetch(
            `${INTERNAL_API_URL}/api/deliveries/available?${backendParams.toString()}`,
            {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${user?.token}`,
                },
            }
        )
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

        // Map backend AdminDeliveryListItemDto to frontend Delivery format
        const mappedData = {
            totalPages: data.totalPages || 1,
            totalCount: data.totalItems || 0,
            items: (data.items || []).map((item: any) => ({
                id: item.id,
                customer: { name: item.clientName || "Unknown Client", id: item.clientId },
                driver: item.driverId ? { id: item.driverId, name: "Assigned" } : null,
                status: item.status,
                createdAt: item.createdAt,
                price: item.price
            }))
        };

        return new Response(JSON.stringify(mappedData), {
            status: 200,
            headers: { "Content-Type": "application/json" },
        });
    } catch (error) {
        console.error("Error fetching deliveries:", error);
        return new Response(JSON.stringify({ message: "Error fetching deliveries" }), {
            status: 500,
            headers: { "Content-Type": "application/json" },
        });
    }
};


export const POST: APIRoute = async (context) => {
    const user = context.locals.user;
    try {
        const body = await context.request.json();
        const response = await fetch(
            `${INTERNAL_API_URL}/api/deliveries/${body.id}/cancel`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ reason: body.reason }),
            }
        );
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
        const responseData = await response.json();
        return new Response(JSON.stringify(responseData), {
            status: 200,
            headers: { "Content-Type": "application/json" },
        });
    } catch (error) {
        console.error("Error fetching deliveries:", error);
        return new Response(JSON.stringify({ message: "Error fetching deliveries" }), {
            status: 500,
            headers: { "Content-Type": "application/json" },
        });
    }
};