import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

export const GET: APIRoute = async (context) => {
    try {
        const user = context.locals.user;
        const url = new URL(context.request.url);
        const objectKey = url.searchParams.get("objectKey");

        if (!objectKey) {
            return new Response(JSON.stringify({ message: "objectKey query parameter is required" }), {
                status: 400,
                headers: { "Content-Type": "application/json" },
            });
        }

        const response = await fetch(
            `${INTERNAL_API_URL}/api/files/view-url?objectKey=${encodeURIComponent(objectKey)}`,
            {
                method: "GET",
                headers: {
                    Authorization: `Bearer ${user?.token}`,
                },
            },
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
        
        const rawData = await response.json();

        return new Response(JSON.stringify(rawData), {
            status: 200,
            headers: {
                "Content-Type": "application/json",
            },
        });
    } catch (error) {
        console.error(error);
        return new Response(JSON.stringify({ message: "something went wrong" }), {
            status: 500,
            headers: {
                "Content-Type": "application/json",
            },
        });
    }
}