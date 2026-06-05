using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wasel.NotificationService.Options;

namespace Wasel.NotificationService.Services;

public class FirebasePushNotificationSender : IPushNotificationSender
{
    private readonly ILogger<FirebasePushNotificationSender> _logger;
    private readonly FirebaseOptions _options;
    private bool _isInitialized;

    public FirebasePushNotificationSender(
        ILogger<FirebasePushNotificationSender> logger,
        IOptions<FirebaseOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        if (!_options.Enabled)
        {
            return;
        }

        try
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                if (string.IsNullOrEmpty(_options.CredentialsPath) || !File.Exists(_options.CredentialsPath))
                {
                    _logger.LogError("Firebase is enabled but credentials file is missing at path: {Path}", _options.CredentialsPath);
                    return;
                }

                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(_options.CredentialsPath),
                    ProjectId = _options.ProjectId
                });
            }
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize FirebaseApp");
        }
    }

    public async Task<PushSendResult> SendAsync(
        string deviceToken,
        string title,
        string message,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isInitialized)
        {
            return new PushSendResult { Success = false, ErrorMessage = "Firebase is not initialized." };
        }

        var fcmMessage = new Message
        {
            Token = deviceToken,
            Notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = title,
                Body = message
            },
            Data = data ?? new Dictionary<string, string>()
        };

        try
        {
            var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(fcmMessage, cancellationToken);
            
            var maskedToken = deviceToken.Length > 10 ? $"{deviceToken[..5]}...{deviceToken[^5..]}" : "***";
            _logger.LogInformation("Successfully sent push notification. MessageId: {MessageId}, Token: {Token}", messageId, maskedToken);
            
            return new PushSendResult
            {
                Success = true,
                MessageId = messageId
            };
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Firebase messaging failed for token starting with {TokenPrefix}", deviceToken.Length >= 5 ? deviceToken[..5] : "***");
            return new PushSendResult
            {
                Success = false,
                ErrorMessage = $"Firebase Error: {ex.Message} (Code: {ex.MessagingErrorCode})"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending push notification.");
            return new PushSendResult
            {
                Success = false,
                ErrorMessage = $"Unexpected Error: {ex.Message}"
            };
        }
    }
}
