import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

export const GET: APIRoute = async (context) => {
    try {
        const user = context.locals.user;

        // 1. Grab the exact query string the React app sent 
        // Example: "?page=1&search=ahmed&statusFilter=APPROVED"
        const queryParams = new URL(context.request.url).search;

        // 2. Append it to the .NET URL
        const response = await fetch(
            `${INTERNAL_API_URL}/api/admin/drivers${queryParams}`,
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