namespace Wasel.Api.Modules.Deliveries.DTOs;

public class AddressResponseDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? AdditionalInfo { get; set; }
    public string? Instructions { get; set; }
    public DateTime CreatedAt { get; set; }
}