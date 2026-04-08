using Amazon.S3;
using Amazon.S3.Model;

namespace CourierPortal.Infrastructure.Services;

public class FileStorageService
{
    private readonly string? _bucketName;
    private readonly string _localBasePath;
    private readonly IAmazonS3? _s3;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IConfiguration config, ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME")
            ?? config["Storage:S3BucketName"];
        _localBasePath = config["Storage:BasePath"] ?? "uploads";

        if (!string.IsNullOrEmpty(_bucketName))
        {
            try
            {
                _s3 = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast2);
                _logger.LogInformation("S3 storage configured: bucket={Bucket}", _bucketName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create S3 client — falling back to local storage");
                _s3 = null;
            }
        }
        else
        {
            _logger.LogInformation("No S3_BUCKET_NAME set — using local file storage at {Path}", _localBasePath);
        }
    }

    public bool IsS3Enabled => _s3 != null && !string.IsNullOrEmpty(_bucketName);

    public async Task<(string FilePath, string FileName)> SaveAsync(IFormFile file, string? folder = null)
    {
        var originalName = file.FileName;
        var key = $"{folder?.TrimEnd('/') ?? "documents"}/{Guid.NewGuid()}{Path.GetExtension(originalName)}";

        if (IsS3Enabled)
        {
            await using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType
            };
            await _s3!.PutObjectAsync(request);
            _logger.LogInformation("Uploaded to S3: {Key}", key);
            return ($"s3://{_bucketName}/{key}", originalName);
        }

        // Local fallback
        var localDir = Path.Combine(_localBasePath, folder ?? "documents");
        Directory.CreateDirectory(localDir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalName)}";
        var filePath = Path.Combine(localDir, fileName);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);
        _logger.LogInformation("Saved locally: {Path}", filePath);
        return (filePath, originalName);
    }

    public async Task<Stream?> GetAsync(string filePath)
    {
        if (filePath.StartsWith("s3://") && IsS3Enabled)
        {
            var key = filePath.Replace($"s3://{_bucketName}/", "");
            var response = await _s3!.GetObjectAsync(_bucketName, key);
            return response.ResponseStream;
        }

        if (File.Exists(filePath))
            return File.OpenRead(filePath);

        return null;
    }

    public async Task DeleteAsync(string filePath)
    {
        if (filePath.StartsWith("s3://") && IsS3Enabled)
        {
            var key = filePath.Replace($"s3://{_bucketName}/", "");
            await _s3!.DeleteObjectAsync(_bucketName, key);
            _logger.LogInformation("Deleted from S3: {Key}", key);
            return;
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Deleted locally: {Path}", filePath);
        }
    }
}
