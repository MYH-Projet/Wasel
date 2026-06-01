public class CreateParcelRequestDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
    public bool IsFragile { get; set; }
    public string? Instructions { get; set; }
}