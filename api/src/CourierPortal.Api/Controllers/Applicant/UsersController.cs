using CourierPortal.Api.Core.Domain;
using CourierPortal.Api.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Api.Controllers.Applicant;

[ApiController]
[Route("api/[controller]")]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await db.AdminUsers.OrderBy(u => u.CreatedDate).Select(u => new {
            u.Id, u.Username, u.Email, u.DisplayName, u.Role, u.IsActive, u.CreatedDate, u.LastLoginDate,
            Status = !u.IsActive ? "Inactive" : u.PasswordHash == null ? "Invited" : "Active",
            HasInvite = u.InviteToken != null && u.InviteExpiry > DateTime.UtcNow
        }).ToListAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
    {
        if (await db.AdminUsers.AnyAsync(u => u.Username == req.Username))
            return BadRequest("Username already exists");

        var user = new AdminUser
        {
            Username = req.Username,
            Email = req.Email,
            DisplayName = req.DisplayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = req.Role,
            IsActive = true
        };
        db.AdminUsers.Add(user);
        await db.SaveChangesAsync();
        return Ok(new { user.Id, user.Username, user.Email, user.DisplayName, user.Role, user.IsActive, user.CreatedDate });
    }

    [HttpPost("invite")]
    public async Task<IActionResult> Invite([FromBody] InviteRequest req)
    {
        var token = Guid.NewGuid().ToString("N");
        var user = new AdminUser
        {
            Username = req.Email,
            Email = req.Email,
            DisplayName = req.DisplayName,
            Role = req.Role ?? "Reviewer",
            InviteToken = token,
            InviteExpiry = DateTime.UtcNow.AddDays(7)
        };
        db.AdminUsers.Add(user);
        await db.SaveChangesAsync();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(new { user = new { user.Id, user.Email, user.DisplayName, user.Role, user.IsActive, user.CreatedDate }, inviteLink = $"{baseUrl}/admin/setup-password?token={token}" });
    }

    [HttpPost("setup-password")]
    public async Task<IActionResult> SetupPassword([FromBody] SetupPasswordRequest req)
    {
        var user = await db.AdminUsers.FirstOrDefaultAsync(u => u.InviteToken == req.Token && u.InviteExpiry > DateTime.UtcNow);
        if (user == null) return BadRequest("Invalid or expired invite link");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        user.InviteToken = null;
        user.InviteExpiry = null;
        await db.SaveChangesAsync();
        return Ok(new { success = true });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req)
    {
        var user = await db.AdminUsers.FindAsync(id);
        if (user is null) return NotFound();
        if (req.DisplayName != null) user.DisplayName = req.DisplayName;
        if (req.Role != null) user.Role = req.Role;
        if (req.IsActive.HasValue) user.IsActive = req.IsActive.Value;
        if (!string.IsNullOrEmpty(req.Email)) user.Email = req.Email;
        await db.SaveChangesAsync();
        return Ok(new { user.Id, user.Username, user.Email, user.DisplayName, user.Role, user.IsActive, user.CreatedDate });
    }

    [HttpPost("{id}/reset-link")]
    public async Task<IActionResult> GenerateResetLink(int id)
    {
        var user = await db.AdminUsers.FindAsync(id);
        if (user is null) return NotFound();
        user.ResetToken = Guid.NewGuid().ToString("N");
        user.ResetExpiry = DateTime.UtcNow.AddDays(7);
        await db.SaveChangesAsync();
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(new { resetLink = $"{baseUrl}/admin/reset-password?token={user.ResetToken}" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await db.AdminUsers.FindAsync(id);
        if (user is null) return NotFound();
        user.IsActive = false;
        await db.SaveChangesAsync();
        return Ok();
    }
}

public record CreateUserRequest(string Username, string Email, string Password, string Role, string? DisplayName = null);
public record UpdateUserRequest(string? Role, bool? IsActive, string? Email, string? DisplayName = null);
public record InviteRequest(string Email, string? DisplayName, string? Role);
public record SetupPasswordRequest(string Token, string Password);
