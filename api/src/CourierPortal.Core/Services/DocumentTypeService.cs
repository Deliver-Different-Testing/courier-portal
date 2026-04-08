using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class DocumentTypeService : IDocumentTypeService
{
    private readonly CourierPortalContext _db;

    public DocumentTypeService(CourierPortalContext db) => _db = db;

    public async Task<List<DocumentTypeDto>> GetAllAsync(bool includeInactive = false)
    {
        var query = _db.DocumentTypes.AsQueryable();
        if (!includeInactive) query = query.Where(d => d.IsActive);
        return await query.OrderBy(d => d.SortOrder).Select(d => MapDto(d)).ToListAsync();
    }

    public async Task<DocumentTypeDto?> GetByIdAsync(int id)
    {
        var entity = await _db.DocumentTypes.FindAsync(id);
        return entity is null ? null : MapDto(entity);
    }

    public async Task<DocumentTypeDto> CreateAsync(CreateDocumentTypeDto dto)
    {
        var entity = new DocumentType
        {
            Name = dto.Name,
            Description = dto.Description,
            IsRequired = dto.IsRequired,
            RenewalMonths = dto.RenewalMonths,
            Category = dto.Category,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive
        };
        _db.DocumentTypes.Add(entity);
        await _db.SaveChangesAsync();
        return MapDto(entity);
    }

    public async Task<DocumentTypeDto?> UpdateAsync(int id, UpdateDocumentTypeDto dto)
    {
        var entity = await _db.DocumentTypes.FindAsync(id);
        if (entity is null) return null;

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IsRequired = dto.IsRequired;
        entity.RenewalMonths = dto.RenewalMonths;
        entity.Category = dto.Category;
        entity.SortOrder = dto.SortOrder;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return MapDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.DocumentTypes.FindAsync(id);
        if (entity is null) return false;
        _db.DocumentTypes.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    private static DocumentTypeDto MapDto(DocumentType d) => new(
        d.Id, d.Name, d.Description, d.IsRequired, d.RenewalMonths, d.Category, d.SortOrder, d.IsActive
    );
}
