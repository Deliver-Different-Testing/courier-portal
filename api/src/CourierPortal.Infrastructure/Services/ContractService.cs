using Amazon.S3;
using Amazon.S3.Model;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CourierPortal.Infrastructure.Services;

public class ContractService : IContractService
{
    private readonly AgentsDbContext _db;
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;

    public ContractService(AgentsDbContext db, IAmazonS3 s3, IConfiguration config)
    {
        _db = db;
        _s3 = s3;
        _bucketName = config["AWS:S3:BucketName"] ?? "dfrnt-documents";
    }

    public async Task<IReadOnlyList<ContractDto>> GetContractsAsync(int tenantId)
    {
        return await _db.CourierContracts
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.CreatedDate)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<ContractDto> UploadContractAsync(CreateContractDto dto, Stream fileStream, string fileName, string mimeType, long fileSize, int tenantId, string? uploadedBy)
    {
        var maxVersion = await _db.CourierContracts
            .Where(c => c.TenantId == tenantId)
            .MaxAsync(c => (int?)c.Version) ?? 0;

        var s3Key = $"contracts/{tenantId}/{Guid.NewGuid()}/{fileName}";

        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = s3Key,
            InputStream = fileStream,
            ContentType = mimeType,
        });

        var contract = new CourierContract
        {
            TenantId = tenantId,
            Name = dto.Name,
            S3Key = s3Key,
            FileName = fileName,
            MimeType = mimeType,
            FileSize = fileSize,
            UploadedBy = uploadedBy,
            Version = maxVersion + 1,
            IsActive = false,
        };

        _db.CourierContracts.Add(contract);
        await _db.SaveChangesAsync();
        return MapToDto(contract);
    }

    public async Task<(Stream Stream, string FileName, string MimeType)?> DownloadContractAsync(int id, int tenantId)
    {
        var c = await _db.CourierContracts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (c is null) return null;

        var response = await _s3.GetObjectAsync(_bucketName, c.S3Key);
        return (response.ResponseStream, c.FileName, c.MimeType);
    }

    public async Task<ContractDto?> ActivateContractAsync(int id, int tenantId)
    {
        var contracts = await _db.CourierContracts.Where(c => c.TenantId == tenantId).ToListAsync();
        var target = contracts.FirstOrDefault(c => c.Id == id);
        if (target is null) return null;

        foreach (var c in contracts) c.IsActive = c.Id == id;
        await _db.SaveChangesAsync();
        return MapToDto(target);
    }

    public async Task<bool> DeleteContractAsync(int id, int tenantId)
    {
        var c = await _db.CourierContracts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (c is null) return false;

        try { await _s3.DeleteObjectAsync(_bucketName, c.S3Key); } catch { /* best effort */ }

        _db.CourierContracts.Remove(c);
        await _db.SaveChangesAsync();
        return true;
    }

    private static ContractDto MapToDto(CourierContract c) => new(
        c.Id, c.TenantId, c.Name, c.FileName, c.MimeType, c.FileSize,
        c.UploadedDate, c.UploadedBy, c.IsActive, c.Version, c.CreatedDate
    );
}
