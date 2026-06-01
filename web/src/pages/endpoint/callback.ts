import type { APIRoute } from "astro";

export const GET: APIRoute = async ({ request, cookies, redirect }) => {
    // 1. Grab the "code" from the URL
    const url = new URL(request.url);
    const code = url.searchParams.get("code");

    if (!code) {
        return new Response("Missing authorization code", { status: 400 });
    }

    // 2. Setup our secrets to talk to Keycloak directly from the server
    // Note: We use the internal Docker name 'wasel-keycloak' here!
    const KEYCLOAK_INTERNAL_URL = import.meta.env.KEYCLOAK_INTERNAL_URL || "http://wasel-keycloak:8080/auth";
    const REALM = import.meta.env.PUBLIC_KEYCLOAK_REALM || "wasel";
    const TOKEN_ENDPOINT = `${KEYCLOAK_INTERNAL_URL}/realms/${REALM}/protocol/openid-connect/token`;
    const CLIENT_ID = import.meta.env.PUBLIC_KEYCLOAK_CLIENT_ID || "wasel-front";
    const APP_URL = import.meta.env.PUBLIC_APP_URL || "http://localhost:8000";
    const INTERNAL_API_URL = import.meta.env.INTERNAL_API_URL || "http://wasel-api:8080";
    const REDIRECT_URI = `${APP_URL}/endpoint/callback`; // Must match exactly

    try {
        // 3. Make the Back-Channel request to swap the code for the JWT
        const params = new URLSearchParams();
        params.append("grant_type", "authorization_code");
        params.append("client_id", CLIENT_ID);
        params.append("code", code);
        params.append("redirect_uri", REDIRECT_URI);

        const response = await fetch(TOKEN_ENDPOINT, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: params
        });

        const data = await response.json();

        if (!response.ok) {
            console.error("Token exchange failed:", data);
            return redirect("/login?error=auth_failed");
        }

        // 4. We got the JWT! Save it as an HttpOnly cookie
        cookies.set("access_token", data.access_token, {
            path: "/",
            httpOnly: true,
            secure: false, // Set to true if using HTTPS in production
            sameSite: "lax",
            maxAge: data.expires_in
        });

        cookies.set("refresh_token", data.refresh_token, {
            path: "/",
            httpOnly: true,
            secure: false,
            sameSite: "lax",
            maxAge: data.refresh_expires_in
        });

        await fetch(`${INTERNAL_API_URL}/api/auth/me`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${data.access_token}`
            }
        });

        // 5. Send the user to the dashboard. They are now officially logged in!
        return redirect("/admin");

    } catch (error) {
        console.error("Callback error:", error);
        return redirect("/login?error=server_error");
    }
};