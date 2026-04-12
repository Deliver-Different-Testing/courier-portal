namespace CourierPortal.Core.DTOs;

/// <summary>NP user list item.</summary>
public record UserListDto(
    int NpUserId,
    string Name,
    string Email,
    string? Phone,
    string Role,
    bool IsActive,
    bool InviteSent,
    DateTime? LastLoginUtc,
    DateTime CreatedDate
);

/// <summary>Create a new NP portal user (sends invitation).</summary>
public record CreateUserDto(
    string Name,
    string Email,
    string? Phone,
    string Role
);

/// <summary>Update an existing NP user.</summary>
public record UpdateUserDto(
    string? Name,
    string? Phone,
    string? Role,
    bool? IsActive
);

/// <summary>Available NP user roles.</summary>
public record UserRoleDto(
    string RoleCode,
    string DisplayName,
    string Description
);
