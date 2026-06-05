import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

function mapBackendStatusToFrontend(backendStatus: string): string {
    switch (backendStatus) {
        case "CREATED":
        case "WAITING_DRIVER":
        case "ASSIGNED":
            return "PENDING";
        case "ACCEPTED":
        case "ARRIVED_AT_PICKUP":
            return "ACCEPTED";
        case "PICKED_UP":
            return "PICKED_UP";
        case "IN_TRANSIT":
        case "ARRIVED_AT_DROPOFF":
            return "IN_TRANSIT";
        case "DELIVERED":
            return "DELIVERED";
        case "CANCELLED_BY_CLIENT":
        case "CANCELLED_BY_DRIVER":
        case "CANCELLED_BY_ADMIN":
            return "CANCELLED";
        default:
            return backendStatus;
    }
}

function mapFrontendStatusToBackend(frontendStatus: string): string | null {
    switch (frontendStatus) {
        case "PENDING":
            return "CREATED,WAITING_DRIVER,ASSIGNED";
        case "ACCEPTED":
            return "ACCEPTED,ARRIVED_AT_PICKUP";
        case "PICKED_UP":
            return "PICKED_UP";
        case "IN_TRANSIT":
            return "IN_TRANSIT,ARRIVED_AT_DROPOFF";
        case "DELIVERED":
            return "DELIVERED";
        case "CANCELLED":
            return "CANCELLED_BY_CLIENT,CANCELLED_BY_DRIVER,CANCELLED_BY_ADMIN";
        default:
            return null;
    }
}

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

        const frontendStatus = searchParams.get("status");
        if (frontendStatus) {
            const mappedStatus = mapFrontendStatusToBackend(frontendStatus);
            if (mappedStatus) {
                backendParams.set("status", mappedStatus);
            }
        }

        const dateFrom = searchParams.get("dateFrom");
        if (dateFrom) {
            backendParams.set("startDate", dateFrom);
        }

        const dateTo = searchParams.get("dateTo");
        if (dateTo) {
            backendParams.set("endDate", dateTo);
        }

        // 2. Call the .NET backend API with the mapped query parameters
        const response = await fetch(
            `${INTERNAL_API_URL}/api/admin/deliveries?${backendParams.toString()}`,
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
        console.log(data);
        // Map backend AdminDeliveryListItemDto to frontend Delivery format
        const mappedData = {
            totalPages: data.totalPages || 1,
            totalCount: data.totalItems || 0,
            items: (data.items || []).map((item: any) => ({
                id: item.id,
                customer: { name: item.clientName || "Unknown Client", id: item.clientId },
                driver: item.driverId ? { id: item.driverId, name: item.driverName } : null,
                status: mapBackendStatusToFrontend(item.status),
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
        console.log(body);
        console.log(user?.token);
        const response = await fetch(
            `${INTERNAL_API_URL}/api/deliveries/${body.id}/cancel`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${user?.token}`,
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