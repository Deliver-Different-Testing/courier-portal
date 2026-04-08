using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class RecruitmentService : IRecruitmentService
{
    private readonly CourierPortalContext _db;

    public RecruitmentService(CourierPortalContext db) => _db = db;

    public async Task<List<JobPostingDto>> GetAllPostingsAsync(string? status = null)
    {
        var query = _db.JobPostings.Include(p => p.Applications).AsQueryable();
        if (!string.IsNullOrEmpty(status)) query = query.Where(p => p.Status == status);
        return await query.Select(p => MapPosting(p)).ToListAsync();
    }

    public async Task<JobPostingDto?> GetPostingByIdAsync(int id)
    {
        var entity = await _db.JobPostings.Include(p => p.Applications).FirstOrDefaultAsync(p => p.Id == id);
        return entity is null ? null : MapPosting(entity);
    }

    public async Task<JobPostingDto> CreatePostingAsync(CreateJobPostingDto dto)
    {
        var entity = new JobPosting
        {
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            VehicleType = dto.VehicleType,
            PayRate = dto.PayRate,
            Status = dto.Status,
            ClosesAt = dto.ClosesAt,
            PostedAt = dto.Status == "active" ? DateTime.UtcNow : null
        };
        _db.JobPostings.Add(entity);
        await _db.SaveChangesAsync();
        return MapPosting(entity);
    }

    public async Task<JobPostingDto?> UpdatePostingAsync(int id, UpdateJobPostingDto dto)
    {
        var entity = await _db.JobPostings.FindAsync(id);
        if (entity is null) return null;

        var wasNotActive = entity.Status != "active";
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Location = dto.Location;
        entity.VehicleType = dto.VehicleType;
        entity.PayRate = dto.PayRate;
        entity.Status = dto.Status;
        entity.ClosesAt = dto.ClosesAt;
        if (wasNotActive && dto.Status == "active") entity.PostedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetPostingByIdAsync(id);
    }

    public async Task<bool> DeletePostingAsync(int id)
    {
        var entity = await _db.JobPostings.FindAsync(id);
        if (entity is null) return false;
        _db.JobPostings.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<JobApplicationDto> CreateApplicationAsync(CreateJobApplicationDto dto)
    {
        var entity = new JobApplication
        {
            PostingId = dto.PostingId,
            ApplicantId = dto.ApplicantId
        };
        _db.JobApplications.Add(entity);
        await _db.SaveChangesAsync();
        return MapApplication(entity);
    }

    public async Task<List<JobApplicationDto>> GetApplicationsByPostingAsync(int postingId)
    {
        return await _db.JobApplications
            .Where(a => a.PostingId == postingId)
            .Select(a => MapApplication(a))
            .ToListAsync();
    }

    public async Task<JobApplicationDto?> UpdateApplicationStatusAsync(int id, UpdateJobApplicationStatusDto dto)
    {
        var entity = await _db.JobApplications.FindAsync(id);
        if (entity is null) return null;
        entity.Status = dto.Status;
        await _db.SaveChangesAsync();
        return MapApplication(entity);
    }

    private static JobPostingDto MapPosting(JobPosting p) => new(
        p.Id, p.Title, p.Description, p.Location, p.VehicleType, p.PayRate, p.Status, p.PostedAt, p.ClosesAt,
        p.Applications?.Count ?? 0
    );

    private static JobApplicationDto MapApplication(JobApplication a) => new(
        a.Id, a.PostingId, a.ApplicantId, a.Status, a.AppliedAt
    );
}
