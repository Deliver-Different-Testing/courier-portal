namespace CourierPortal.Core.Domain.Entities;

public class AdminUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string? DisplayName { get; set; }
    public string? PasswordHash { get; set; }
    public string Role { get; set; } = "Reviewer";
    public bool IsActive { get; set; } = true;
    public string? InviteToken { get; set; }
    public DateTime? InviteExpiry { get; set; }
    public string? ResetToken { get; set; }
    public DateTime? ResetExpiry { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
}
