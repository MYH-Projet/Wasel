/// <reference types="astro/client" />

declare namespace App {
    interface Locals {
        // This tells TypeScript that 'user' exists and contains our JWT data
        user: import("jose").JWTPayload & {
            realm_access?: {
                roles?: string[];
            };
            preferred_username?: string;
            email?: string;
            name?: string;
        };
    }
}