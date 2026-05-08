import { defineMiddleware } from "astro:middleware";
import { jwtVerify, createRemoteJWKSet } from "jose";

// 1. Point to your Keycloak 'certs' endpoint (JWKS URL)
// Use the internal container name if running inside Podman
const JWKS_URL = new URL("http://wasel-keycloak:8080/auth/realms/wasel/protocol/openid-connect/certs");

// 2. Create the 'Key Set' handler (This handles kid matching and caching!)
const JWKS = createRemoteJWKSet(JWKS_URL);

export const onRequest = defineMiddleware(async (context, next) => {
    const token = context.cookies.get("access_token")?.value;
    const isAdmin = context.url.pathname.startsWith("/admin");

    if (isAdmin) {
        if (!token) return context.redirect("/login");

        try {
            // 3. Verify the token
            const { payload } = await jwtVerify(token, JWKS, {
                issuer: "http://localhost/auth/realms/wasel"
            });

            if (payload.realm_access?.roles?.includes("ADMIN")) {
                context.locals.user = payload;
            } else {
                throw new Error("You are not authorized to access this page");
            }

        } catch (error) {
            console.error("JWT Verification Failed:", error);
            return context.redirect("/login");
        }
    }

    return next();
});