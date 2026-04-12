using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// User CRUD via tucClientContact + MasterUser. Role assignment
/// (NpAdmin/NpDispatcher/NpReadOnly). Scoped to NP's AgentId.
/// </summary>
public class NpUserService : INpUserService
{
    private readonly AgentsDbContext _db;

    public NpUserService(AgentsDbContext db) => _db = db;

    private static readonly IReadOnlyList<UserRoleDto> Roles = new List<UserRoleDto>
    {
        new("NpAdmin", "Administrator", "Full access to all NP portal features including user management and financials."),
        new("NpDispatcher", "Dispatcher", "Operations and fleet management access. Cannot see financial data or manage users."),
        new("NpReadOnly", "Read Only", "View-only access to dashboard and fleet status. No editing capabilities.")
    }.AsReadOnly();

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserListDto>> GetUsersAsync(int npAgentId)
    {
        return await _db.NpUsers
            .Where(u => u.AgentId == npAgentId && u.RecordStatusId == 1)
            .OrderBy(u => u.Name)
            .Select(u => new UserListDto(
                u.NpUserId, u.Name, u.Email, u.Phone, u.Role,
                u.IsActive, u.InviteSent, u.LastLoginUtc, u.CreatedDate
            ))
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<UserListDto?> GetUserAsync(int npUserId, int npAgentId)
    {
        var u = await _db.NpUsers
            .FirstOrDefaultAsync(x => x.NpUserId == npUserId && x.AgentId == npAgentId && x.RecordStatusId == 1);

        return u is null ? null : new UserListDto(
            u.NpUserId, u.Name, u.Email, u.Phone, u.Role,
            u.IsActive, u.InviteSent, u.LastLoginUtc, u.CreatedDate
        );
    }

    /// <inheritdoc />
    public async Task<UserListDto> CreateUserAsync(CreateUserDto dto, int npAgentId, int tenantId)
    {
        // Validate role
        if (!Roles.Any(r => r.RoleCode == dto.Role))
            throw new InvalidOperationException($"Invalid role '{dto.Role}'. Valid roles: {string.Join(", ", Roles.Select(r => r.RoleCode))}");

        // Check for duplicate email within this NP
        var exists = await _db.NpUsers
            .AnyAsync(u => u.AgentId == npAgentId && u.Email == dto.Email && u.RecordStatusId == 1);
        if (exists)
            throw new InvalidOperationException($"A user with email '{dto.Email}' already exists for this NP.");

        var user = new NpUser
        {
            AgentId = npAgentId,
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Role = dto.Role
        };

        // TODO: In production, also create:
        // 1. tucClientContact record linked to the NP's tucClient
        // 2. MasterUser record for Hub login
        // 3. Send invitation email

        _db.NpUsers.Add(user);
        await _db.SaveChangesAsync();

        return new UserListDto(
            user.NpUserId, user.Name, user.Email, user.Phone, user.Role,
            user.IsActive, user.InviteSent, user.LastLoginUtc, user.CreatedDate
        );
    }

    /// <inheritdoc />
    public async Task<UserListDto?> UpdateUserAsync(int npUserId, UpdateUserDto dto, int npAgentId)
    {
        var u = await _db.NpUsers
            .FirstOrDefaultAsync(x => x.NpUserId == npUserId && x.AgentId == npAgentId && x.RecordStatusId == 1);

        if (u is null) return null;

        if (dto.Name is not null) u.Name = dto.Name;
        if (dto.Phone is not null) u.Phone = dto.Phone;
        if (dto.Role is not null)
        {
            if (!Roles.Any(r => r.RoleCode == dto.Role))
                throw new InvalidOperationException($"Invalid role '{dto.Role}'.");
            u.Role = dto.Role;
        }
        if (dto.IsActive.HasValue) u.IsActive = dto.IsActive.Value;

        u.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new UserListDto(
            u.NpUserId, u.Name, u.Email, u.Phone, u.Role,
            u.IsActive, u.InviteSent, u.LastLoginUtc, u.CreatedDate
        );
    }

    /// <inheritdoc />
    public async Task<bool> DeleteUserAsync(int npUserId, int npAgentId)
    {
        var u = await _db.NpUsers
            .FirstOrDefaultAsync(x => x.NpUserId == npUserId && x.AgentId == npAgentId && x.RecordStatusId == 1);

        if (u is null) return false;

        u.RecordStatusId = 0;
        u.IsActive = false;
        u.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public IReadOnlyList<UserRoleDto> GetAvailableRoles() => Roles;
}
