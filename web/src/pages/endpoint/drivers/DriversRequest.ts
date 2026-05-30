import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

export const GET: APIRoute = async (context) => {
    try {
        const user = context.locals.user;
        const queryParams = new URL(context.request.url).search;
        const response = await fetch(
            `${INTERNAL_API_URL}/api/admin/drivers/pending?${queryParams}`,
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

        const itemsList = Array.isArray(rawData) ? rawData : (rawData.items || []);

        const mappedItems = itemsList.map((item: any) => ({
            id: item.driverId,
            fullName: `${item.firstName} ${item.lastName}`.trim(),
            gmail: `${item.email} `,
            phone: `${item.phone}`,
            licenseNumber: item.permitNumber,
            submissionDate: item.createdAt,
            dossierStatus: item.status === "PendingVerification" ? "SUBMITTED" : "UNDER_REVIEW"
        }));


        const driversRequestsData = Array.isArray(rawData) ? {
            totalPages: 3,
            totalCount: 142,
            items: mappedItems
        } : {
            ...rawData,
            items: mappedItems
        };

        console.log(driversRequestsData);

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

export const PATCH: APIRoute = async (context) => {
    try {
        const user = context.locals.user;
        const body = await context.request.json();

        const operation = await fetch(`${INTERNAL_API_URL}/api/admin/driver-dossiers/${body.dossierId}/status`,
            {
                method: "PATCH",
                headers: {
                    "Authorization": `Bearer ${user?.token}`,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ status: body.action, reason: body.payload?.reason })
            }
        )
        if (!operation.ok) {
            const isJson = operation.headers.get("content-type")?.includes("application/json");
            const errorMessage = isJson
                ? (await operation.json()).message || "Backend error"
                : operation.statusText;

            return new Response(JSON.stringify({ message: errorMessage }), {
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