using CourierPortal.Core.DTOs;

namespace CourierPortal.Core.Interfaces;

public interface IRecruitmentService
{
    Task<List<JobPostingDto>> GetAllPostingsAsync(string? status = null);
    Task<JobPostingDto?> GetPostingByIdAsync(int id);
    Task<JobPostingDto> CreatePostingAsync(CreateJobPostingDto dto);
    Task<JobPostingDto?> UpdatePostingAsync(int id, UpdateJobPostingDto dto);
    Task<bool> DeletePostingAsync(int id);
    Task<JobApplicationDto> CreateApplicationAsync(CreateJobApplicationDto dto);
    Task<List<JobApplicationDto>> GetApplicationsByPostingAsync(int postingId);
    Task<JobApplicationDto?> UpdateApplicationStatusAsync(int id, UpdateJobApplicationStatusDto dto);
}
