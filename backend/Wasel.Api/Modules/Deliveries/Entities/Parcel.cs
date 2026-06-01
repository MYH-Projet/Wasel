using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Deliveries.Entities;

public class Parcel : BaseEntity
{
    public string Description { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public decimal Volume { get; set; }

    public bool IsFragile { get; set; }

    public string? Instructions { get; set; }
}