using CourierPortal.Core.DTOs;

namespace CourierPortal.Core.Interfaces;

public interface IDocumentTypeService
{
    Task<List<DocumentTypeDto>> GetAllAsync(bool includeInactive = false);
    Task<DocumentTypeDto?> GetByIdAsync(int id);
    Task<DocumentTypeDto> CreateAsync(CreateDocumentTypeDto dto);
    Task<DocumentTypeDto?> UpdateAsync(int id, UpdateDocumentTypeDto dto);
    Task<bool> DeleteAsync(int id);
}
