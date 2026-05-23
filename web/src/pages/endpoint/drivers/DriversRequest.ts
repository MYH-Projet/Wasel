import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

export const GET: APIRoute = async (context) => {
    try {
        // const user = context.locals.user;
        // const queryParams = new URL(context.request.url).search;
        // const DriversRequests = await fetch(
        //     `${INTERNAL_API_URL}/api/admin/drivers/pending?${queryParams}`,
        //     {
        //         method: "GET",
        //         headers: {
        //             Authorization: `Bearer ${user?.token}`,
        //         },
        //     },
        // );
        // if (!DriversRequests.ok) {
        //     return new Response(JSON.stringify({ message: "Backend error" }), {
        //         status: DriversRequests.status,
        //         headers: { "Content-Type": "application/json" },
        //     });
        // }
        // const driversRequestsData = await DriversRequests.json();
        await new Promise((resolve) => setTimeout(resolve, 600));

        const driversRequestsData = {
            totalPages: 3,
            totalCount: 24,
            items: [
                {
                    id: "REQ-8942A",
                    fullName: "Youssef Alaoui",
                    cin: "KB123456",
                    licenseNumber: "12/345678",
                    submissionDate: "2026-05-15T08:30:00Z",
                    dossierStatus: "SUBMITTED"
                },
                {
                    id: "REQ-8943B",
                    fullName: "Fatima Zahra Mansouri",
                    cin: "CD98765",
                    licenseNumber: "09/112233",
                    submissionDate: "2026-05-14T14:15:00Z",
                    dossierStatus: "UNDER_REVIEW"
                },
                {
                    id: "REQ-8944C",
                    fullName: "Omar Chraibi",
                    cin: "BJ554433",
                    licenseNumber: "15/998877",
                    submissionDate: "2026-05-14T09:00:00Z",
                    dossierStatus: "SUBMITTED"
                },
                {
                    id: "REQ-8945D",
                    fullName: "Mehdi El Fassi",
                    cin: "A12345",
                    licenseNumber: "05/667788",
                    submissionDate: "2026-05-13T16:45:00Z",
                    dossierStatus: "UNDER_REVIEW"
                },
                {
                    id: "REQ-8946E",
                    fullName: "Amina Bennani",
                    cin: "Z998877",
                    licenseNumber: "22/445566",
                    submissionDate: "2026-05-13T10:20:00Z",
                    dossierStatus: "SUBMITTED"
                }
            ]
        };
        return new Response(JSON.stringify(driversRequestsData), {
            status: 200,
            headers: {
                "Content-Type": "application/json",
            },
        });
    } catch (error) {
        console.log(error);
        return new Response(JSON.stringify({ message: "something went wrong" }), {
            status: 500,
            headers: {
                "Content-Type": "application/json",
            },
        });
    }
}

export const POST: APIRoute = async (context) => {
    try {
        const user = context.locals.user;
        const body = await context.request.json();
        const action = body.action;

        const operation = await fetch(`${INTERNAL_API_URL}/api/admin/drivers/${body.id}/${action}`,
            {
                method: "POST",
                headers: {
                    "Authorization": `Bearer ${user?.token}`,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(body.payload)
            }
        )
        if (!operation.ok) {
            return new Response(JSON.stringify({ message: "Backend error" }), {
                status: operation.status,
                headers: { "Content-Type": "application/json" },
            });
        }

        const res = await operation.json();

        return new Response(JSON.stringify(res), {
            status: 200,
            headers: {
                "Content-Type": "application/json",
            },
        });
    }
    catch (error) {
        console.log(error);
        return new Response(JSON.stringify({ message: "something went wrong" }), {
            status: 500,
            headers: {
                "Content-Type": "application/json",
            },
        });
    }
}