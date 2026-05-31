using System.ComponentModel.DataAnnotations;

namespace Wasel.Api.Modules.Deliveries.DTOs;

public class DeliveryEstimateRequestDto
{
    [Required]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double PickupLat { get; set; }

    [Required]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double PickupLng { get; set; }

    [Required]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double DropoffLat { get; set; }

    [Required]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double DropoffLng { get; set; }

    [Required]
    [Range(0.01, 100, ErrorMessage = "Weight must be greater than 0 and maximum 100 kg")]
    public double Weight { get; set; }

    [Required]
    public bool IsFragile { get; set; }
}
