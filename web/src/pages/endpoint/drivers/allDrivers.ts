import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

export const GET: APIRoute = async (context) => {
    try {
        const user = context.locals.user;

        // // 1. Grab the exact query string the React app sent 
        // // Example: "?page=1&search=ahmed&statusFilter=APPROVED"
        // const queryParams = new URL(context.request.url).search;

        // // 2. Append it to the .NET URL
        // const response = await fetch(
        //     `${INTERNAL_API_URL}/api/admin/drivers${queryParams}`,
        //     {
        //         method: "GET",
        //         headers: {
        //             Authorization: `Bearer ${user?.token}`,
        //         },
        //     },
        // );

        // // 3. Safety check!
        // if (!response.ok) {
        // return new Response(JSON.stringify({ message: "Backend error" }), {
        //     status: response.status,
        //     headers: { "Content-Type": "application/json" },
        // });
        // }
        // const data = await response.json();
        // ⏳ Simulate network delay so you can see your beautiful "Chargement..." state
        await new Promise(resolve => setTimeout(resolve, 600));

        // ✅ 2. RETURN THIS MOCK DATA
        const mockData = {
            totalPages: 4,
            totalCount: 38,
            items: [
                {
                    id: "DRV-1001",
                    fullName: "Ahmed Benali",
                    email: "ahmed.benali@email.ma",
                    phone: "+212 6 00 11 22 33",
                    driverStatus: "APPROVED",
                    dossierStatus: "APPROVED",
                    registrationDate: "2025-11-20T10:30:00Z",
                    missionCount: 145
                },
                {
                    id: "DRV-1002",
                    fullName: "Khadija Idrissi",
                    email: "k.idrissi@email.ma",
                    phone: "+212 6 99 88 77 66",
                    driverStatus: "PENDING_VERIFICATION",
                    dossierStatus: "UNDER_REVIEW",
                    registrationDate: "2026-05-15T14:20:00Z",
                    missionCount: 0
                },
                {
                    id: "DRV-1003",
                    fullName: "Youssef Naciri",
                    email: "y.naciri@email.ma",
                    phone: "+212 6 55 44 33 22",
                    driverStatus: "SUSPENDED",
                    dossierStatus: "APPROVED",
                    registrationDate: "2025-08-10T09:15:00Z",
                    missionCount: 89
                },
                {
                    id: "DRV-1004",
                    fullName: "Fatima El Fassi",
                    email: "fatima.fassi@email.ma",
                    phone: "+212 6 11 22 33 44",
                    driverStatus: "REJECTED",
                    dossierStatus: "REJECTED",
                    registrationDate: "2026-05-10T16:45:00Z",
                    missionCount: 0
                },
                {
                    id: "DRV-1005",
                    fullName: "Omar Chraibi",
                    email: "omar.c@email.ma",
                    phone: "+212 6 77 88 99 00",
                    driverStatus: "APPROVED",
                    dossierStatus: "APPROVED",
                    registrationDate: "2026-02-05T11:00:00Z",
                    missionCount: 42
                }
            ]
        };

        const data = mockData;

        return new Response(JSON.stringify(data), {
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