import type { APIRoute } from "astro";

export const GET: APIRoute = async (context) => {
    // ⏳ Simulate network delay for the loading state
    await new Promise((resolve) => setTimeout(resolve, 600));

    // ✅ MOCK DATA FOR USERS
    const mockData = {
        totalPages: 8,
        totalCount: 142,
        items: [
            {
                id: "USR-1001",
                fullName: "Youssef Alaoui",
                email: "youssef.alaoui@gmail.com",
                phone: "+212 6 11 22 33 44",
                status: "ACTIVE",
                activeRole: "DRIVER",
                createdAt: "2025-11-20T10:30:00Z"
            },
            {
                id: "USR-1002",
                fullName: "Acme Logistics Corp",
                email: "contact@acme.ma",
                phone: "+212 5 22 33 44 55",
                status: "ACTIVE",
                activeRole: "CUSTOMER",
                createdAt: "2026-01-15T14:20:00Z"
            },
            {
                id: "USR-1004",
                fullName: "Karim Idrissi",
                email: "karim.idrissi@gmail.com",
                phone: "+212 6 77 88 99 00",
                status: "BLOCKED",
                activeRole: "CUSTOMER",
                createdAt: "2026-04-10T16:45:00Z"
            },
            {
                id: "USR-1005",
                fullName: "Omar Chraibi",
                email: "omar.c@email.ma",
                phone: "+212 6 99 88 77 66",
                status: "ACTIVE",
                activeRole: "DRIVER",
                createdAt: "2026-05-05T11:00:00Z"
            }
        ]
    };

    return new Response(JSON.stringify(mockData), {
        status: 200,
        headers: { "Content-Type": "application/json" },
    });
};