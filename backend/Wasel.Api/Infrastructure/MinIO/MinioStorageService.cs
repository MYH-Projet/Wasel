using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Wasel.Api.Infrastructure.MinIO;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioOptions _options;

    public MinioStorageService(IMinioClient minioClient, IOptions<MinioOptions> options)
    {
        _minioClient = minioClient;
        _options = options.Value;
    }

    public async Task<string> GenerateUploadUrlAsync(string objectKey, string contentType, TimeSpan expiry)
    {
        await EnsureBucketExistsAsync();

        var args = new PresignedPutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithExpiry(ToExpirySeconds(expiry));

        return await _minioClient.PresignedPutObjectAsync(args);
    }

    public async Task<string> GenerateViewUrlAsync(string objectKey, TimeSpan expiry)
    {
        await EnsureBucketExistsAsync();

        var args = new PresignedGetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithExpiry(ToExpirySeconds(expiry));

        return await _minioClient.PresignedGetObjectAsync(args);
    }

    public async Task EnsureBucketExistsAsync()
    {
        var bucketExistsArgs = new BucketExistsArgs()
            .WithBucket(_options.BucketName);

        var exists = await _minioClient.BucketExistsAsync(bucketExistsArgs);
        if (exists)
        {
            return;
        }

        var makeBucketArgs = new MakeBucketArgs()
            .WithBucket(_options.BucketName);

        await _minioClient.MakeBucketAsync(makeBucketArgs);
    }

    private static int ToExpirySeconds(TimeSpan expiry)
    {
        return Math.Max(1, Convert.ToInt32(expiry.TotalSeconds));
    }
}
