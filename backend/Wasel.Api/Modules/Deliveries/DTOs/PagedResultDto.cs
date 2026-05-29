namespace Wasel.Api.Modules.Deliveries.DTOs;

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}