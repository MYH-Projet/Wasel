import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

export const PATCH: APIRoute = async (context) => {
    try {
        const user = context.locals.user;
        const body = await context.request.json();

        // 2. Append it to the .NET URL
        const response = await fetch(
            `${INTERNAL_API_URL}/api/admin/drivers/${body.driverId}/status`,
            {
                method: "PATCH",
                headers: {
                    Authorization: `Bearer ${user?.token}`,
                },
                body: JSON.stringify({
                    action: body.action,
                    payload: body.payload
                }),
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


        return new Response(JSON.stringify(data), {
            status: 200,
            headers: { "Content-Type": "application/json" },
        });

    } catch (error) {
        console.error(error);
        return new Response(JSON.stringify({ message: "Serveur inaccessible" }), {
            status: 500,
            headers: { "Content-Type": "application/json" },
        });
    }
}