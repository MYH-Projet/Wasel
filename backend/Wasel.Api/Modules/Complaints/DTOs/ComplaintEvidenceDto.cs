namespace Wasel.Api.Modules.Complaints.DTOs;

public class ComplaintEvidenceDto
{
    public Guid Id { get; set; }
    public string ObjectKey { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}