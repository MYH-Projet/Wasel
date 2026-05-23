import type { APIRoute } from "astro";

export const GET: APIRoute = async (context) => {
    // ⏳ Simulate network delay
    await new Promise((resolve) => setTimeout(resolve, 600));

    // ✅ MOCK DATA FOR DELIVERIES
    const mockData = {
        totalPages: 5,
        totalCount: 42,
        items: [
            {
                id: "DEL-9001",
                customerName: "Youssef Alaoui",
                driverName: "Ahmed Benali",
                status: "IN_TRANSIT",
                createdAt: "2026-05-23T10:30:00Z",
                price: 45.00
            },
            {
                id: "DEL-9002",
                customerName: "Acme Corp (B2B)",
                driverName: "Julian Rossi",
                status: "PICKED_UP",
                createdAt: "2026-05-23T14:15:00Z",
                price: 120.50
            },
            {
                id: "DEL-9003",
                customerName: "Sara Mansouri",
                driverName: null, // Unassigned
                status: "PENDING",
                createdAt: "2026-05-23T15:00:00Z",
                price: 30.00
            },
            {
                id: "DEL-9004",
                customerName: "Karim Idrissi",
                driverName: "Omar Chraibi",
                status: "DELIVERED",
                createdAt: "2026-05-22T09:45:00Z",
                price: 55.00
            },
            {
                id: "DEL-9005",
                customerName: "Nour El Fassi",
                driverName: "Khadija Idrissi",
                status: "ACCEPTED",
                createdAt: "2026-05-23T14:50:00Z",
                price: 40.00
            }
        ]
    };

    return new Response(JSON.stringify(mockData), {
        status: 200,
        headers: { "Content-Type": "application/json" },
    });
};