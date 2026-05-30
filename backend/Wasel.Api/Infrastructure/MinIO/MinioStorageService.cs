using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Wasel.Api.Infrastructure.MinIO;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _internalClient;
    private readonly IMinioClient _publicClient;
    private readonly MinioOptions _options;

    public MinioStorageService(IOptions<MinioOptions> options)
    {
        _options = options.Value;

        _internalClient = new MinioClient()
            .WithEndpoint(_options.InternalEndpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(_options.UseSSL)
            .Build();

        _publicClient = new MinioClient()
            .WithEndpoint(_options.PublicEndpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(_options.UseSSL)
            .Build();
    }

    public async Task<string> GenerateUploadUrlAsync(string objectKey, string contentType, TimeSpan expiry)
    {
        await EnsureBucketExistsAsync();

        var args = new PresignedPutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithExpiry(ToExpirySeconds(expiry));

        return await _publicClient.PresignedPutObjectAsync(args);
    }

    public async Task<string> GenerateViewUrlAsync(string objectKey, TimeSpan expiry)
    {
        await EnsureBucketExistsAsync();

        var args = new PresignedGetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithExpiry(ToExpirySeconds(expiry));

        return await _publicClient.PresignedGetObjectAsync(args);
    }

    public async Task EnsureBucketExistsAsync()
    {
        var bucketExistsArgs = new BucketExistsArgs()
            .WithBucket(_options.BucketName);

        var exists = await _internalClient.BucketExistsAsync(bucketExistsArgs);
        if (exists)
        {
            return;
        }

        var makeBucketArgs = new MakeBucketArgs()
            .WithBucket(_options.BucketName);

        await _internalClient.MakeBucketAsync(makeBucketArgs);
    }

    private static int ToExpirySeconds(TimeSpan expiry)
    {
        return Math.Max(1, Convert.ToInt32(expiry.TotalSeconds));
    }
}
