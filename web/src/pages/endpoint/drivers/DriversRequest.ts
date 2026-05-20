import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL;

export const GET: APIRoute = async (context) => {
    let driversRequestsData: any[] = [];
    try {
        const user = context.locals.user;
        const queryParams = new URL(context.request.url).search;
        const DriversRequests = await fetch(
            `${INTERNAL_API_URL}/api/admin/drivers/pending?${queryParams}`,
            {
                method: "GET",
                headers: {
                    Authorization: `Bearer ${user?.token}`,
                },
            },
        );
        if (!DriversRequests.ok) {
            return new Response(JSON.stringify({ message: "Backend error" }), {
                status: DriversRequests.status,
                headers: { "Content-Type": "application/json" },
            });
        }
        driversRequestsData = await DriversRequests.json();
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