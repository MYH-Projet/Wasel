import type { APIRoute } from "astro";

const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL
export const GET: APIRoute = async (context) => {
    const user = context.locals.user;
    const DriversRequests = await fetch(`${INTERNAL_API_URL}/api/admin/drivers/requests`,
        {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${user?.token}`
            }
        }
    ).then(res => res.json());

    return new Response(JSON.stringify(DriversRequests), {
        status: 200,
        headers: {
            "Content-Type": "application/json",
        },
    });
}

export const POST: APIRoute = async (context) => {
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

    const res = await operation.json();

    if (!operation.ok) {
        return new Response(JSON.stringify(res), {
            //the error code from backend
            status: res.code,
            headers: {
                "Content-Type": "application/json",
            },
        });
    }
    return new Response(JSON.stringify(res), {
        status: 200,
        headers: {
            "Content-Type": "application/json",
        },
    });
}