using System.Text.Json;
using FluentAssertions;
using Wasel.NotificationService.DTOs;
using Xunit;

namespace Wasel.NotificationService.Tests;

public class NotificationRequestedEventTests
{
    [Fact]
    public void Should_Deserialize_Correctly()
    {
        var json = @"
        {
            ""eventId"": ""11111111-1111-1111-1111-111111111111"",
            ""recipientUserId"": ""22222222-2222-2222-2222-222222222222"",
            ""type"": ""DELIVERY_ASSIGNED"",
            ""title"": ""Nouvelle livraison"",
            ""message"": ""Message"",
            ""channels"": [""IN_APP"", ""PUSH""],
            ""createdAt"": ""2026-06-04T10:00:00Z""
        }";

        var result = JsonSerializer.Deserialize<NotificationRequestedEvent>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.EventId.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        result.RecipientUserId.Should().Be(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        result.Type.Should().Be("DELIVERY_ASSIGNED");
        result.Channels.Should().ContainInOrder("IN_APP", "PUSH");
    }
}
