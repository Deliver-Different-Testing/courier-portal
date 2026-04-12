using Amazon.S3;
using Amazon.S3.Model;
using CourierPortal.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CourierPortal.Infrastructure.Services;

public class DocumentStorageConfig
{
    public string BucketName { get; set; } = "staging-dfrnt-documents";
    public string Region { get; set; } = "us-east-1";
    public int MaxFileSizeMb { get; set; } = 10;
    public int PresignedUrlExpiryMinutes { get; set; } = 15;
    public string[] AllowedMimeTypes { get; set; } = ["application/pdf", "image/jpeg", "image/png", "image/bmp"];
}

public class S3DocumentStorageService : IDocumentStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly DocumentStorageConfig _config;
    private readonly ILogger<S3DocumentStorageService> _logger;

    public S3DocumentStorageService(IAmazonS3 s3, IConfiguration configuration, ILogger<S3DocumentStorageService> logger)
    {
        _s3 = s3;
        _logger = logger;
        _config = new DocumentStorageConfig();
        configuration.GetSection("DocumentStorage").Bind(_config);
    }

    public string BuildCourierDocumentKey(int tenantId, int courierId, int docId, string fileName)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.');
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"tenants/{tenantId}/courier-documents/{courierId}/{docId}-{timestamp}.{ext}";
    }

    public string BuildTemplateKey(int tenantId, int docTypeId, string fileName)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.');
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"tenants/{tenantId}/document-templates/{docTypeId}-{timestamp}.{ext}";
    }

    public async Task<string> UploadAsync(Stream stream, string s3Key, string contentType)
    {
        _logger.LogInformation("Uploading document to S3: {Key}", s3Key);

        var request = new PutObjectRequest
        {
            BucketName = _config.BucketName,
            Key = s3Key,
            InputStream = stream,
            ContentType = contentType,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        await _s3.PutObjectAsync(request);
        return s3Key;
    }

    public async Task<Stream> DownloadAsync(string s3Key)
    {
        _logger.LogInformation("Downloading document from S3: {Key}", s3Key);

        var response = await _s3.GetObjectAsync(new GetObjectRequest
        {
            BucketName = _config.BucketName,
            Key = s3Key
        });

        return response.ResponseStream;
    }

    public Task<string> GetPresignedUrlAsync(string s3Key, int expiryMinutes = 15)
    {
        var effectiveExpiry = expiryMinutes > 0 ? expiryMinutes : _config.PresignedUrlExpiryMinutes;

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _config.BucketName,
            Key = s3Key,
            Expires = DateTime.UtcNow.AddMinutes(effectiveExpiry),
            Verb = HttpVerb.GET
        };

        var url = _s3.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    public async Task DeleteAsync(string s3Key)
    {
        _logger.LogInformation("Deleting document from S3: {Key}", s3Key);

        await _s3.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _config.BucketName,
            Key = s3Key
        });
    }
}
