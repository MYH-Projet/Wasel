import type { APIRoute } from "astro";

export const POST: APIRoute = async ({ cookies, redirect }) => {
    const refreshToken = cookies.get("refresh_token")?.value;

    const KEYCLOAK_INTERNAL_URL =
        (typeof process !== "undefined" ? process.env.KEYCLOAK_INTERNAL_URL : undefined) ||
        import.meta.env.KEYCLOAK_INTERNAL_URL ||
        "http://wasel-keycloak-staging:8080/auth";
    const REALM = import.meta.env.PUBLIC_KEYCLOAK_REALM || "wasel";
    const CLIENT_ID = import.meta.env.PUBLIC_KEYCLOAK_CLIENT_ID || "wasel-front";
    const LOGOUT_ENDPOINT = `${KEYCLOAK_INTERNAL_URL}/realms/${REALM}/protocol/openid-connect/logout`;

    // 1. Revoke the session on Keycloak's side (back-channel logout)
    //    This invalidates the refresh token so it can't be reused.
    if (refreshToken) {
        try {
            await fetch(LOGOUT_ENDPOINT, {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: new URLSearchParams({
                    client_id: CLIENT_ID,
                    refresh_token: refreshToken,
                }),
            });
        } catch (error) {
            // Log but don't block — we still clear cookies regardless
            console.error("Keycloak back-channel logout failed:", error);
        }
    }

    // 2. Clear the httpOnly cookies (JS cannot do this — must be server-side)
    cookies.delete("access_token", { path: "/" });
    cookies.delete("refresh_token", { path: "/" });

    // 3. Redirect to login
    return redirect("/login");
};
