using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Wasel.NotificationService.DTOs;
using Wasel.NotificationService.Entities;
using Wasel.NotificationService.Enums;
using Wasel.NotificationService.Repositories;
using Wasel.NotificationService.Services;
using Xunit;

namespace Wasel.NotificationService.Tests;

public class NotificationProcessorTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly Mock<IPushNotificationSender> _pushSenderMock;
    private readonly Mock<ILogger<NotificationProcessor>> _loggerMock;
    private readonly NotificationProcessor _processor;

    public NotificationProcessorTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _pushSenderMock = new Mock<IPushNotificationSender>();
        _loggerMock = new Mock<ILogger<NotificationProcessor>>();

        _processor = new NotificationProcessor(
            _repositoryMock.Object,
            _pushSenderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessAsync_Should_Save_Notification_And_Set_Created_When_InApp_Only()
    {
        // Arrange
        var request = new NotificationRequestedEvent
        {
            EventId = Guid.NewGuid(),
            RecipientUserId = Guid.NewGuid(),
            Type = "DELIVERY_ASSIGNED",
            Title = "Title",
            Message = "Message",
            Channels = new[] { "IN_APP" }
        };

        Notification? savedNotification = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, c) => savedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _processor.ProcessAsync(request);

        // Assert
        savedNotification.Should().NotBeNull();
        savedNotification!.Status.Should().Be(NotificationStatus.CREATED);
        savedNotification.Channel.Should().Be("IN_APP");
        _pushSenderMock.Verify(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_Should_Set_NoDeviceToken_When_No_Active_Device_Token()
    {
        // Arrange
        var request = new NotificationRequestedEvent
        {
            EventId = Guid.NewGuid(),
            RecipientUserId = Guid.NewGuid(),
            Type = "DELIVERY_ASSIGNED",
            Title = "Title",
            Message = "Message",
            Channels = new[] { "PUSH" }
        };

        _repositoryMock.Setup(r => r.GetActiveDeviceTokensAsync(request.RecipientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDeviceToken>());

        Notification? savedNotification = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, c) => savedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _processor.ProcessAsync(request);

        // Assert
        savedNotification.Should().NotBeNull();
        savedNotification!.Status.Should().Be(NotificationStatus.NO_DEVICE_TOKEN);
        _pushSenderMock.Verify(p => p.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_Should_Set_Sent_When_Push_Succeeds()
    {
        // Arrange
        var request = new NotificationRequestedEvent
        {
            EventId = Guid.NewGuid(),
            RecipientUserId = Guid.NewGuid(),
            Type = "DELIVERY_ASSIGNED",
            Title = "Title",
            Message = "Message",
            Channels = new[] { "PUSH" }
        };

        _repositoryMock.Setup(r => r.GetActiveDeviceTokensAsync(request.RecipientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDeviceToken> { new UserDeviceToken { Token = "token1" } });

        _pushSenderMock.Setup(p => p.SendAsync("token1", request.Title, request.Message, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PushSendResult { Success = true, MessageId = "msg1" });

        Notification? savedNotification = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, c) => savedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _processor.ProcessAsync(request);

        // Assert
        savedNotification!.Status.Should().Be(NotificationStatus.SENT);
        savedNotification.FirebaseMessageId.Should().Be("msg1");
        savedNotification.SentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessAsync_Should_Set_Failed_When_PushSender_Fails()
    {
        // Arrange
        var request = new NotificationRequestedEvent
        {
            EventId = Guid.NewGuid(),
            RecipientUserId = Guid.NewGuid(),
            Type = "DELIVERY_ASSIGNED",
            Title = "Title",
            Message = "Message",
            Channels = new[] { "PUSH" }
        };

        _repositoryMock.Setup(r => r.GetActiveDeviceTokensAsync(request.RecipientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserDeviceToken> { new UserDeviceToken { Token = "token1" } });

        _pushSenderMock.Setup(p => p.SendAsync("token1", request.Title, request.Message, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PushSendResult { Success = false, ErrorMessage = "Firebase error" });

        Notification? savedNotification = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, c) => savedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _processor.ProcessAsync(request);

        // Assert
        savedNotification!.Status.Should().Be(NotificationStatus.FAILED);
        savedNotification.ErrorMessage.Should().Be("Firebase error");
        savedNotification.SentAt.Should().BeNull();
    }
}
