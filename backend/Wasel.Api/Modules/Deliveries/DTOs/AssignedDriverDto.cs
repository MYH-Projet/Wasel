namespace Wasel.Api.Modules.Deliveries.DTOs;

public class AssignedDriverDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public double? AverageRating { get; set; }
    public VehicleInfoDto? Vehicle { get; set; }
}