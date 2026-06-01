using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Wasel.Api.Modules.Drivers.DTOs;

public class VehicleRequestDto
{
    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("matricule")]
    public string Matricule { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("marque")]
    public string Marque { get; set; } = string.Empty;
}
