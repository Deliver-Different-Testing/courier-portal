namespace CourierPortal.Core.DTOs;

public record JobPostingDto(int Id, string Title, string? Description, string? Location, string? VehicleType, decimal? PayRate, string Status, DateTime? PostedAt, DateTime? ClosesAt, int ApplicationCount);
public record CreateJobPostingDto(string Title, string? Description, string? Location, string? VehicleType, decimal? PayRate, string Status, DateTime? ClosesAt);
public record UpdateJobPostingDto(string Title, string? Description, string? Location, string? VehicleType, decimal? PayRate, string Status, DateTime? ClosesAt);
public record JobApplicationDto(int Id, int PostingId, int ApplicantId, string Status, DateTime AppliedAt);
public record CreateJobApplicationDto(int PostingId, int ApplicantId);
public record UpdateJobApplicationStatusDto(string Status);
