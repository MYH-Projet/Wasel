namespace Wasel.Api.Infrastructure.Firebase;

/// <summary>
/// Configuration options for Firebase Cloud Messaging.
/// Not used in this branch (Noop sender), but prepared for future integration.
/// </summary>
public class FirebaseOptions
{
    public const string SectionName = "Firebase";

    /// <summary>Whether Firebase push notifications are enabled.</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>Firebase project ID.</summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Path to the Firebase service account credentials JSON file.</summary>
    public string CredentialsPath { get; set; } = string.Empty;
}
