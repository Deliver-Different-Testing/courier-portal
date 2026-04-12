using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CourierPortal.Infrastructure.Services;

public class DocumentTypeService : IDocumentTypeService
{
    private readonly AgentsDbContext _db;
    private readonly IDocumentStorageService _storage;
    private readonly ILogger<DocumentTypeService> _logger;

    public DocumentTypeService(AgentsDbContext db, IDocumentStorageService storage, ILogger<DocumentTypeService> logger)
    {
        _db = db;
        _storage = storage;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DocumentTypeListDto>> GetAllAsync(int tenantId)
    {
        return await _db.CourierDocumentTypes
            .Where(dt => dt.TenantId == tenantId && dt.Active)
            .OrderBy(dt => dt.SortOrder)
            .ThenBy(dt => dt.Name)
            .Select(dt => new DocumentTypeListDto(
                dt.Id,
                dt.Name,
                dt.Instructions,
                dt.Category,
                dt.Mandatory,
                dt.Active,
                dt.HasExpiry,
                dt.ExpiryWarningDays,
                dt.BlockOnExpiry,
                dt.AppliesTo,
                dt.SortOrder,
                dt.TemplateS3Key != null
            ))
            .ToListAsync();
    }

    public async Task<DocumentTypeDetailDto?> GetByIdAsync(int id, int tenantId)
    {
        var dt = await _db.CourierDocumentTypes
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId);

        if (dt == null) return null;

        return MapToDetail(dt);
    }

    public async Task<DocumentTypeDetailDto> CreateAsync(CreateDocumentTypeDto dto, int tenantId)
    {
        var entity = new CourierDocumentType
        {
            TenantId = tenantId,
            Name = dto.Name,
            Instructions = dto.Instructions,
            Category = dto.Category,
            Mandatory = dto.Mandatory,
            HasExpiry = dto.HasExpiry,
            ExpiryWarningDays = dto.ExpiryWarningDays,
            BlockOnExpiry = dto.BlockOnExpiry,
            AppliesTo = dto.AppliesTo,
            SortOrder = dto.SortOrder,
            Active = true,
            CreatedDate = DateTime.UtcNow
        };

        _db.CourierDocumentTypes.Add(entity);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created document type {Id} '{Name}' for tenant {TenantId}", entity.Id, entity.Name, tenantId);
        return MapToDetail(entity);
    }

    public async Task<DocumentTypeDetailDto?> UpdateAsync(int id, UpdateDocumentTypeDto dto, int tenantId)
    {
        var entity = await _db.CourierDocumentTypes
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId);

        if (entity == null) return null;

        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.Instructions != null) entity.Instructions = dto.Instructions;
        if (dto.Category.HasValue) entity.Category = dto.Category.Value;
        if (dto.Mandatory.HasValue) entity.Mandatory = dto.Mandatory.Value;
        if (dto.Active.HasValue) entity.Active = dto.Active.Value;
        if (dto.HasExpiry.HasValue) entity.HasExpiry = dto.HasExpiry.Value;
        if (dto.ExpiryWarningDays.HasValue) entity.ExpiryWarningDays = dto.ExpiryWarningDays.Value;
        if (dto.BlockOnExpiry.HasValue) entity.BlockOnExpiry = dto.BlockOnExpiry.Value;
        if (dto.AppliesTo.HasValue) entity.AppliesTo = dto.AppliesTo.Value;
        if (dto.SortOrder.HasValue) entity.SortOrder = dto.SortOrder.Value;
        entity.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDetail(entity);
    }

    public async Task<bool> DeactivateAsync(int id, int tenantId)
    {
        var entity = await _db.CourierDocumentTypes
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId);

        if (entity == null) return false;

        entity.Active = false;
        entity.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<string> UploadTemplateAsync(int id, int tenantId, Stream stream, string fileName, string mimeType)
    {
        var entity = await _db.CourierDocumentTypes
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId)
            ?? throw new InvalidOperationException($"Document type {id} not found for tenant {tenantId}");

        // Delete old template if exists
        if (!string.IsNullOrEmpty(entity.TemplateS3Key))
        {
            await _storage.DeleteAsync(entity.TemplateS3Key);
        }

        var s3Key = _storage.BuildTemplateKey(tenantId, id, fileName);
        await _storage.UploadAsync(stream, s3Key, mimeType);

        entity.TemplateS3Key = s3Key;
        entity.TemplateFileName = fileName;
        entity.TemplateMimeType = mimeType;
        entity.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return s3Key;
    }

    public async Task<(Stream Stream, string FileName, string MimeType)?> DownloadTemplateAsync(int id, int tenantId)
    {
        var entity = await _db.CourierDocumentTypes
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId);

        if (entity?.TemplateS3Key == null || entity.TemplateFileName == null || entity.TemplateMimeType == null)
            return null;

        var stream = await _storage.DownloadAsync(entity.TemplateS3Key);
        return (stream, entity.TemplateFileName, entity.TemplateMimeType);
    }

    public async Task SeedDefaultTypesAsync(int tenantId)
    {
        var existing = await _db.CourierDocumentTypes.AnyAsync(dt => dt.TenantId == tenantId);
        if (existing) return;

        var defaults = new (string Name, DocumentCategory Category, bool HasExpiry, bool Mandatory, bool BlockOnExpiry, int Sort)[]
        {
            ("Driver's License", DocumentCategory.Licensing, true, true, true, 1),
            ("Vehicle Registration", DocumentCategory.Vehicle, true, true, true, 2),
            ("Insurance Certificate", DocumentCategory.Insurance, true, true, true, 3),
            ("WOF", DocumentCategory.Vehicle, true, true, true, 4),
            ("DG Certificate", DocumentCategory.Licensing, true, false, false, 5),
            ("Contract", DocumentCategory.Contract, false, true, false, 6),
        };

        foreach (var (name, category, hasExpiry, mandatory, block, sort) in defaults)
        {
            _db.CourierDocumentTypes.Add(new CourierDocumentType
            {
                TenantId = tenantId,
                Name = name,
                Category = category,
                HasExpiry = hasExpiry,
                Mandatory = mandatory,
                BlockOnExpiry = block,
                SortOrder = sort,
                Active = true,
                AppliesTo = DocumentAppliesTo.Both,
                ExpiryWarningDays = 30,
                CreatedDate = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} default document types for tenant {TenantId}", defaults.Length, tenantId);
    }

    private static DocumentTypeDetailDto MapToDetail(CourierDocumentType dt) => new(
        dt.Id, dt.TenantId, dt.Name, dt.Instructions, dt.Category,
        dt.Mandatory, dt.Active, dt.HasExpiry, dt.ExpiryWarningDays, dt.BlockOnExpiry,
        dt.TemplateFileName, dt.TemplateMimeType, dt.AppliesTo, dt.SortOrder,
        dt.CreatedDate, dt.ModifiedDate
    );
}
