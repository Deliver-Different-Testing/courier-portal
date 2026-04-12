using CourierPortal.Core.DTOs;

namespace CourierPortal.Core.Interfaces;

/// <summary>Courier recruitment pipeline management.</summary>
public interface IRecruitmentService
{
    Task<ApplicantDetailDto> CreateApplicantAsync(CreateApplicantDto dto, int tenantId);
    Task<ApplicantDetailDto?> UpdateApplicantAsync(int id, UpdateApplicantDto dto, int tenantId);
    Task<IReadOnlyList<ApplicantListDto>> GetApplicantsAsync(int tenantId, string? stage = null, string? search = null, DateTime? from = null, DateTime? to = null);
    Task<ApplicantDetailDto?> GetApplicantByIdAsync(int id, int tenantId);
    Task<ApplicantDetailDto?> AdvanceStageAsync(int id, int tenantId);
    Task<ApplicantDetailDto?> RejectApplicantAsync(int id, RejectApplicantDto dto, int tenantId);
    Task<ApplicantDetailDto?> ApproveApplicantAsync(int id, ApproveApplicantDto dto, int tenantId);
    Task<bool> DeleteApplicantAsync(int id, int tenantId);
    Task<IReadOnlyList<RecruitmentPipelineSummaryDto>> GetPipelineSummaryAsync(int tenantId);

    /// <summary>Promote an approved applicant to a full courier record.</summary>
    Task<int> PromoteApplicantToCourierAsync(int applicantId, int tenantId);
}

/// <summary>Recruitment stage configuration CRUD.</summary>
public interface IRecruitmentStageService
{
    Task<IReadOnlyList<RecruitmentStageConfigDto>> GetStagesAsync(int tenantId);
    Task<RecruitmentStageConfigDto> CreateStageAsync(CreateStageConfigDto dto, int tenantId);
    Task<RecruitmentStageConfigDto?> UpdateStageAsync(int id, UpdateStageConfigDto dto, int tenantId);
    Task<bool> DeleteStageAsync(int id, int tenantId);
    Task<IReadOnlyList<RecruitmentStageConfigDto>> SeedDefaultsAsync(int tenantId);
}

/// <summary>Contract template management.</summary>
public interface IContractService
{
    Task<IReadOnlyList<ContractDto>> GetContractsAsync(int tenantId);
    Task<ContractDto> UploadContractAsync(CreateContractDto dto, Stream fileStream, string fileName, string mimeType, long fileSize, int tenantId, string? uploadedBy);
    Task<(Stream Stream, string FileName, string MimeType)?> DownloadContractAsync(int id, int tenantId);
    Task<ContractDto?> ActivateContractAsync(int id, int tenantId);
    Task<bool> DeleteContractAsync(int id, int tenantId);
}
