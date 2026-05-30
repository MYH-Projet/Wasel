namespace Wasel.Api.Modules.Messaging.DTOs;

public class SendMessageRequestDto
{
    public Guid DeliveryId { get; set; }
    public string Content { get; set; } = string.Empty;
}