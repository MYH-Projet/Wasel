import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

export const GET: APIRoute = async (context) => {
    try {
        const user = context.locals.user;

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
            // so Enum.TryParse in C# can parse it case-insensitively.
            const cleanStatus = statusFilter.replace(/_/g, "");
            backendParams.set("driverStatus", cleanStatus);
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
            `${INTERNAL_API_URL}/api/admin/drivers?${backendParams.toString()}`,
            {
                method: "GET",
                headers: {
                    Authorization: `Bearer ${user?.token}`,
                },
            },
        );

        // 3. Safety check!
        if (!response.ok) {
            const isJson = response.headers.get("content-type")?.includes("application/json");
            const errorMessage = isJson
                ? (await response.json()).message || "Backend error"
                : response.statusText; // Fallback for Nginx HTML errors (e.g., "Bad Gateway")

            return new Response(JSON.stringify({ message: errorMessage }), {
                status: response.status,
                headers: { "Content-Type": "application/json" },
            });
        }
        const data = await response.json();
        console.log("🚀 ~ data:", data)

        // Map the backend data to the frontend's expected schema
        const mappedData = {
            totalPages: data.totalPages,
            totalCount: data.totalItems,
            items: (data.items || []).map((item: any) => ({
                id: item.driverId,
                fullName: `${item.firstName} ${item.lastName}`.trim(),
                email: item.email,
                phone: item.phone,
                driverStatus: item.driverStatus?.toUpperCase(),
                dossierStatus: item.dossierStatus?.toUpperCase(),
                registrationDate: item.createdAt
            }))
        };

        return new Response(JSON.stringify(mappedData), {
            status: 200,
            headers: { "Content-Type": "application/json" },
        });

    } catch (error) {
        console.error(error);
        // Remember: don't stringify the raw error!
        return new Response(JSON.stringify({ message: "Serveur inaccessible" }), {
            status: 500,
            headers: { "Content-Type": "application/json" },
        });
    }
}