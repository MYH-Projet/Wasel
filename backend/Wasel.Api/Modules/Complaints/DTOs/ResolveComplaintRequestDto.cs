namespace Wasel.Api.Modules.Complaints.DTOs;

public class ResolveComplaintRequestDto
{
    public string ResolutionType { get; set; } = string.Empty;
    public string? AdminComment { get; set; }
    public decimal? ApprovedAmount { get; set; }
}