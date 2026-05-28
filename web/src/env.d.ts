type KeycloakPayload = import('./middleware').KeycloakPayload;

declare namespace App {
    interface Locals {
        user: KeycloakPayload;
    }
}