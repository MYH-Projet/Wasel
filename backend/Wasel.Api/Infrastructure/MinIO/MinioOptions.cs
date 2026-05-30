namespace Wasel.Api.Infrastructure.MinIO;

public class MinioOptions
{
    public const string SectionName = "MinIO";

    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "wasel-files";
    public bool UseSSL { get; set; }
}
