namespace Wasel.Api.Infrastructure.MinIO;

public interface IStorageService
{
    Task<string> GenerateUploadUrlAsync(string objectKey, string contentType, TimeSpan expiry);
    Task<string> GenerateViewUrlAsync(string objectKey, TimeSpan expiry);
    Task EnsureBucketExistsAsync();
}
