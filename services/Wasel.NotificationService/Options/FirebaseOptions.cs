namespace Wasel.NotificationService.Options;

public class FirebaseOptions
{
    public const string SectionName = "Firebase";

    public bool Enabled { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string CredentialsPath { get; set; } = string.Empty;
}
