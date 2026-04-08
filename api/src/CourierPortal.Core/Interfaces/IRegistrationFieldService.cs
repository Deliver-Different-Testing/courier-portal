using CourierPortal.Core.DTOs;

namespace CourierPortal.Core.Interfaces;

public interface IRegistrationFieldService
{
    Task<List<RegistrationFieldDto>> GetAllFieldsAsync();
    Task<List<RegistrationFieldDto>> GetActiveFieldsAsync();
    Task<RegistrationFieldDto?> GetFieldByIdAsync(int id);
    Task<RegistrationFieldDto> CreateFieldAsync(CreateRegistrationFieldDto dto);
    Task<RegistrationFieldDto?> UpdateFieldAsync(int id, UpdateRegistrationFieldDto dto);
    Task<bool> DeleteFieldAsync(int id);
    Task<RegistrationFieldOptionDto> AddOptionAsync(int fieldId, CreateRegistrationFieldOptionDto dto);
    Task<bool> RemoveOptionAsync(int optionId);
}
