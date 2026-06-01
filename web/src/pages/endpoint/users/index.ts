import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

export const GET: APIRoute = async (context) => {

    const user = context.locals.user;
    try {
        const queryParams = new URL(context.request.url).search;
        const response = await fetch(`${INTERNAL_API_URL}/api/admin/users${queryParams}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${user?.token}`
            }
        });

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

        // 2. Happy Path (2xx)
        const data = await response.json();
        return new Response(JSON.stringify(data), {
            status: 200,
            headers: { "Content-Type": "application/json" },
        });

    } catch (err: any) {
        console.error("BFF Fetch Error:", err);
        return new Response(JSON.stringify({ message: "Serveur inaccessible" }), {
            status: 500,
            headers: { "Content-Type": "application/json" },
        });
    }



    // // ⏳ Simulate network delay for the loading state
    // await new Promise((resolve) => setTimeout(resolve, 600));

    // // ✅ MOCK DATA FOR USERS
    // const mockData = {
    //     totalPages: 8,
    //     totalCount: 142,
    //     items: [
    //         {
    //             id: "USR-1001",
    //             fullName: "Youssef Alaoui",
    //             email: "youssef.alaoui@gmail.com",
    //             phone: "+212 6 11 22 33 44",
    //             status: "ACTIVE",
    //             activeRole: "DRIVER",
    //             createdAt: "2025-11-20T10:30:00Z"
    //         },
    //         {
    //             id: "USR-1002",
    //             fullName: "Acme Logistics Corp",
    //             email: "contact@acme.ma",
    //             phone: "+212 5 22 33 44 55",
    //             status: "ACTIVE",
    //             activeRole: "CUSTOMER",
    //             createdAt: "2026-01-15T14:20:00Z"
    //         },
    //         {
    //             id: "USR-1004",
    //             fullName: "Karim Idrissi",
    //             email: "karim.idrissi@gmail.com",
    //             phone: "+212 6 77 88 99 00",
    //             status: "BLOCKED",
    //             activeRole: "CUSTOMER",
    //             createdAt: "2026-04-10T16:45:00Z"
    //         },
    //         {
    //             id: "USR-1005",
    //             fullName: "Omar Chraibi",
    //             email: "omar.c@email.ma",
    //             phone: "+212 6 99 88 77 66",
    //             status: "ACTIVE",
    //             activeRole: "DRIVER",
    //             createdAt: "2026-05-05T11:00:00Z"
    //         }
    //     ]
    // };

    // return new Response(JSON.stringify(mockData), {
    //     status: 200,
    //     headers: { "Content-Type": "application/json" },
    // });
};


export const PATCH: APIRoute = async (context) => {
    const user = context.locals.user;
    try {
        const body = await context.request.json();
        const { id, status, reason } = body;
        const response = await fetch(`${INTERNAL_API_URL}/api/admin/users/${id}/status`, {
            method: "PATCH",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${user?.token}`

            },
            body: JSON.stringify({
                status
            })
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
            headers: { "Content-Type": "application/json" },
        });

    } catch (err: any) {
        console.error("BFF Fetch Error:", err);
        return new Response(JSON.stringify({ message: "Serveur inaccessible" }), {
            status: 500,
            headers: { "Content-Type": "application/json" },
        });
    }
};