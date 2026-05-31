using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Wasel.Api.Modules.Drivers.DTOs;

public class RegisterDriverRequestDto
{
    [Required]
    [JsonPropertyName("permisNumber")]
    public string PermitNumber { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("vehicle")]
    public VehicleRequestDto Vehicle { get; set; } = null!;
}
