using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class RegistrationFieldService : IRegistrationFieldService
{
    private readonly CourierPortalContext _db;

    public RegistrationFieldService(CourierPortalContext db) => _db = db;

    public async Task<List<RegistrationFieldDto>> GetAllFieldsAsync()
    {
        return await _db.RegistrationFields
            .Include(f => f.Options)
            .OrderBy(f => f.SortOrder)
            .Select(f => MapField(f))
            .ToListAsync();
    }

    public async Task<List<RegistrationFieldDto>> GetActiveFieldsAsync()
    {
        return await _db.RegistrationFields
            .Include(f => f.Options)
            .Where(f => f.IsVisible)
            .OrderBy(f => f.SortOrder)
            .Select(f => MapField(f))
            .ToListAsync();
    }

    public async Task<RegistrationFieldDto?> GetFieldByIdAsync(int id)
    {
        var entity = await _db.RegistrationFields.Include(f => f.Options).FirstOrDefaultAsync(f => f.Id == id);
        return entity is null ? null : MapField(entity);
    }

    public async Task<RegistrationFieldDto> CreateFieldAsync(CreateRegistrationFieldDto dto)
    {
        var entity = new RegistrationField
        {
            FieldName = dto.FieldName,
            FieldType = dto.FieldType,
            IsRequired = dto.IsRequired,
            IsVisible = dto.IsVisible,
            SortOrder = dto.SortOrder,
            Section = dto.Section,
            HelpText = dto.HelpText
        };
        _db.RegistrationFields.Add(entity);
        await _db.SaveChangesAsync();
        return MapField(entity);
    }

    public async Task<RegistrationFieldDto?> UpdateFieldAsync(int id, UpdateRegistrationFieldDto dto)
    {
        var entity = await _db.RegistrationFields.FindAsync(id);
        if (entity is null) return null;

        entity.FieldName = dto.FieldName;
        entity.FieldType = dto.FieldType;
        entity.IsRequired = dto.IsRequired;
        entity.IsVisible = dto.IsVisible;
        entity.SortOrder = dto.SortOrder;
        entity.Section = dto.Section;
        entity.HelpText = dto.HelpText;
        await _db.SaveChangesAsync();
        return await GetFieldByIdAsync(id);
    }

    public async Task<bool> DeleteFieldAsync(int id)
    {
        var entity = await _db.RegistrationFields.FindAsync(id);
        if (entity is null) return false;
        _db.RegistrationFields.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<RegistrationFieldOptionDto> AddOptionAsync(int fieldId, CreateRegistrationFieldOptionDto dto)
    {
        var entity = new RegistrationFieldOption
        {
            FieldId = fieldId,
            OptionText = dto.OptionText,
            SortOrder = dto.SortOrder
        };
        _db.RegistrationFieldOptions.Add(entity);
        await _db.SaveChangesAsync();
        return new RegistrationFieldOptionDto(entity.Id, entity.FieldId, entity.OptionText, entity.SortOrder);
    }

    public async Task<bool> RemoveOptionAsync(int optionId)
    {
        var entity = await _db.RegistrationFieldOptions.FindAsync(optionId);
        if (entity is null) return false;
        _db.RegistrationFieldOptions.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    private static RegistrationFieldDto MapField(RegistrationField f) => new(
        f.Id, f.FieldName, f.FieldType, f.IsRequired, f.IsVisible, f.SortOrder, f.Section, f.HelpText,
        f.Options?.OrderBy(o => o.SortOrder).Select(o => new RegistrationFieldOptionDto(o.Id, o.FieldId, o.OptionText, o.SortOrder)).ToList()
    );
}
