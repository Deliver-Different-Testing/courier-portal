namespace CourierPortal.Core.DTOs;

public record CreateApplicantDto(
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string? Address,
    string? City,
    string? State,
    string? Postcode,
    string? VehicleType,
    string? VehicleMake,
    string? VehicleModel,
    int? VehicleYear,
    string? VehiclePlate,
    string? BankAccountName,
    string? BankAccountNumber,
    string? BankBSB,
    string? NextOfKinName,
    string? NextOfKinPhone,
    string? NextOfKinRelationship,
    string? Notes
);

public record UpdateApplicantDto(
    string? Email,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? Address,
    string? City,
    string? State,
    string? Postcode,
    string? VehicleType,
    string? VehicleMake,
    string? VehicleModel,
    int? VehicleYear,
    string? VehiclePlate,
    string? BankAccountName,
    string? BankAccountNumber,
    string? BankBSB,
    string? NextOfKinName,
    string? NextOfKinPhone,
    string? NextOfKinRelationship,
    string? Notes
);

public record ApplicantListDto(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string PipelineStage,
    string? VehicleType,
    bool DeclarationSigned,
    DateTime CreatedDate,
    DateTime? ModifiedDate,
    int? ApprovedAsCourierId
);

public record ApplicantDetailDto(
    int Id,
    int TenantId,
    int? RegionId,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string? Address,
    string? City,
    string? State,
    string? Postcode,
    string? VehicleType,
    string? VehicleMake,
    string? VehicleModel,
    int? VehicleYear,
    string? VehiclePlate,
    string? BankAccountName,
    string? BankAccountNumber,
    string? BankBSB,
    string? NextOfKinName,
    string? NextOfKinPhone,
    string? NextOfKinRelationship,
    string PipelineStage,
    bool DeclarationSigned,
    DateTime? DeclarationSignedDate,
    string? DeclarationSignatureS3Key,
    DateTime? RejectedDate,
    string? RejectedReason,
    int? ApprovedAsCourierId,
    DateTime CreatedDate,
    DateTime? ModifiedDate,
    string? Notes
);

public record ApplicantStageUpdateDto(string Stage);

public record ApproveApplicantDto(
    int AgentId,
    string CourierCode
);

public record RejectApplicantDto(string Reason);
