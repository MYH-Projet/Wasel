namespace Wasel.Api.Modules.Deliveries.Enums
{
    public enum DeliveryStatus
    {
        CREATED,
        WAITING_DRIVER,
        ASSIGNED,
        ACCEPTED,
        ARRIVED_AT_PICKUP,
        PICKED_UP,
        IN_TRANSIT,
        ARRIVED_AT_DROPOFF,
        DELIVERED,
        CANCELLED_BY_CLIENT,
        CANCELLED_BY_DRIVER,
        CANCELLED_BY_ADMIN,
        PROBLEM_REPORTED
    }
}