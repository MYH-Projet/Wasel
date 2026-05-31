using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Deliveries.Entities;

public class Address : BaseEntity
{
    public Guid ClientId { get; set; }

    public string Label { get; set; } = string.Empty;
    // Exemple : "pickup", "dropoff", "client address"

    public string Street { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? PostalCode { get; set; }

    public string Country { get; set; } = "Morocco";

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? AdditionalInfo { get; set; }

    public User Client { get; set; } = default!;

    public string? Instructions { get; set; }
}