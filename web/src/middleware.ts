import { defineMiddleware } from "astro:middleware";
import { jwtVerify, createRemoteJWKSet } from "jose";
import type { JWTPayload } from "jose";

export interface KeycloakPayload extends JWTPayload {
    realm_access?: {
        roles: string[];
    };
    azp?: string;
    preferred_username?: string;
    email?: string;
    name?: string;
    given_name?: string;
    family_name?: string;
    token?: string;
}

// 1. Point to your Keycloak 'certs' endpoint (JWKS URL)
// Use the internal container name if running inside Podman
const KEYCLOAK_INTERNAL_URL = import.meta.env.KEYCLOAK_INTERNAL_URL || "http://wasel-keycloak:8080/auth";
const KEYCLOAK_REALM = import.meta.env.PUBLIC_KEYCLOAK_REALM || "wasel";
const JWKS_URL = new URL(`${KEYCLOAK_INTERNAL_URL}/realms/${KEYCLOAK_REALM}/protocol/openid-connect/certs`);

// 2. Create the 'Key Set' handler (This handles kid matching and caching!)
const JWKS = createRemoteJWKSet(JWKS_URL);

export const onRequest = defineMiddleware(async (context, next) => {
    const token = context.cookies.get("access_token")?.value;
    const refreshToken = context.cookies.get("refresh_token")?.value;
    const isAdmin = context.url.pathname.startsWith("/admin");

    if (isAdmin) {
        if (!token && !refreshToken) return context.redirect("/login");

        try {
            if (token) {
                console.log("im here in token")
                const baseUrl = import.meta.env.PUBLIC_KEYCLOAK_URL || "/auth";
                const absoluteKeycloakUrl = baseUrl.startsWith('http') ? baseUrl : `${context.url.origin}${baseUrl}`;
                const { payload } = await jwtVerify<KeycloakPayload>(token, JWKS, {
                    issuer: `${absoluteKeycloakUrl}/realms/${KEYCLOAK_REALM}`
                });
                payload.token = token;
                if (payload.realm_access?.roles.includes("ADMIN")) {
                    context.locals.user = payload;
                    return next();
                } else {
                    throw new Error("You are not authorized to access this page");
                }
            }

        } catch (error: any) {
            console.error("JWT Verification Failed:", error);
            if (error.code !== 'ERR_JWT_EXPIRED') {
                return context.redirect("/login");
            }
        }

        if (refreshToken) {
            console.log("im here in refreshToken")
            try {
                const tokenEndpoint = new URL(`${KEYCLOAK_INTERNAL_URL}/realms/${KEYCLOAK_REALM}/protocol/openid-connect/token`);
                const response = await fetch(tokenEndpoint, {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    body: new URLSearchParams({
                        grant_type: "refresh_token",
                        client_id: import.meta.env.PUBLIC_KEYCLOAK_CLIENT_ID || "wasel-front",
                        refresh_token: refreshToken
                    })
                });

                const data = await response.json();
                if (!response.ok) {
                    console.error("Token refresh failed:", data);
                    return context.redirect("/login");
                }
                context.cookies.set("access_token", data.access_token, {
                    path: "/",
                    httpOnly: true,
                    secure: false, // Set to true if using HTTPS in production
                    sameSite: "lax",
                    maxAge: data.expires_in
                });
                context.cookies.set("refresh_token", data.refresh_token, {
                    path: "/",
                    httpOnly: true,
                    secure: false,
                    sameSite: "lax",
                    maxAge: data.refresh_expires_in
                });
                const baseUrl = import.meta.env.PUBLIC_KEYCLOAK_URL || "/auth";
                const absoluteKeycloakUrl = baseUrl.startsWith('http') ? baseUrl : `${context.url.origin}${baseUrl}`;
                const { payload } = await jwtVerify<KeycloakPayload>(data.access_token, JWKS, {
                    issuer: `${absoluteKeycloakUrl}/realms/${KEYCLOAK_REALM}`
                });
                payload.token = data.access_token;
                context.locals.user = payload;
                return next();
            } catch (error: any) {
                console.error("Token refresh failed:", error);
                return context.redirect("/login");
            }
        }
    }
    console.log("im here non")
    return next();
});