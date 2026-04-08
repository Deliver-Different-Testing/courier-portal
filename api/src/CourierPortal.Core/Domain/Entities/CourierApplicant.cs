namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Full applicant record for the courier recruitment pipeline.
/// Tracks profile, vehicle, banking, next of kin, and declaration status.
/// </summary>
public class CourierApplicant
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int? RegionId { get; set; }

    // --- Identity ---
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    // --- Address ---
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Postcode { get; set; }

    // --- Vehicle ---
    public string? VehicleType { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public int? VehicleYear { get; set; }
    public string? VehiclePlate { get; set; }

    // --- Banking ---
    public string? BankAccountName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankBSB { get; set; }

    // --- Next of Kin ---
    public string? NextOfKinName { get; set; }
    public string? NextOfKinPhone { get; set; }
    public string? NextOfKinRelationship { get; set; }

    // --- Pipeline ---
    public string PipelineStage { get; set; } = "Registration";

    // --- Declaration ---
    public bool DeclarationSigned { get; set; }
    public DateTime? DeclarationSignedDate { get; set; }
    public string? DeclarationSignatureS3Key { get; set; }

    // --- Outcome ---
    public DateTime? RejectedDate { get; set; }
    public string? RejectedReason { get; set; }
    public int? ApprovedAsCourierId { get; set; }

    // --- Meta ---
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public NpCourier? ApprovedAsCourier { get; set; }
}
