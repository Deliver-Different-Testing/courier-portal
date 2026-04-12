namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Courier entity scoped to a Network Partner. Mirrors existing tucCourier fields
/// but owned/managed by the NP through the partner portal.
/// </summary>
public class NpCourier
{
    public int CourierId { get; set; }

    /// <summary>The NP agent that owns this courier.</summary>
    public int AgentId { get; set; }

    /// <summary>Tenant ID (inherited from agent).</summary>
    public int TenantId { get; set; }

    // --- Identity ---

    /// <summary>Unique courier code (e.g. "NP-AKL-001").</summary>
    public string Code { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string SurName { get; set; } = string.Empty;

    /// <summary>Master or Sub courier type.</summary>
    public string CourierType { get; set; } = "Master";

    /// <summary>If Sub, the Master courier's ID.</summary>
    public int? MasterId { get; set; }

    /// <summary>Fleet group assignment.</summary>
    public int? FleetId { get; set; }

    // --- Contact ---

    public string? PersonalMobile { get; set; }
    public string? UrgentMobile { get; set; }
    public string? Email { get; set; }
    public string? HomePhone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Postcode { get; set; }

    // --- Next of Kin ---

    public string? NextOfKinName { get; set; }
    public string? NextOfKinPhone { get; set; }
    public string? NextOfKinRelationship { get; set; }

    // --- Vehicle ---

    /// <summary>Vehicle type code (Car, Van, Truck, etc.).</summary>
    public string? VehicleType { get; set; }

    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public int? VehicleYear { get; set; }
    public string? RegoNo { get; set; }
    public int? MaxPallets { get; set; }
    public decimal? VehicleHeight { get; set; }
    public decimal? VehicleWidth { get; set; }
    public decimal? VehicleLength { get; set; }

    // --- Compliance ---

    public string? DriversLicenseNo { get; set; }
    public DateTime? DriversLicenseExpiry { get; set; }
    public bool HasDangerousGoods { get; set; }
    public DateTime? DangerousGoodsExpiry { get; set; }
    public DateTime? WofExpiry { get; set; }
    public DateTime? RegoExpiry { get; set; }
    public string? TslNumber { get; set; }
    public DateTime? TslExpiry { get; set; }
    public string? HteNumber { get; set; }
    public DateTime? HteExpiry { get; set; }

    // --- Insurance ---

    public string? InsurancePolicyNumber { get; set; }
    public string? InsuranceCompany { get; set; }
    public decimal? CarrierLiability { get; set; }
    public decimal? PublicLiability { get; set; }
    public DateTime? InsuranceExpiry { get; set; }

    // --- Financial ---

    public string? IrdNumber { get; set; }
    public string? GstNumber { get; set; }
    public decimal? WithholdingTaxPercent { get; set; }
    public string? BankAccount { get; set; }
    public decimal? PaymentPercent { get; set; }
    public decimal? BonusPercent { get; set; }

    // --- Device & Settings ---

    /// <summary>Communication channel (App, SMS, Web).</summary>
    public string? Channel { get; set; }

    public string? DeviceType { get; set; }
    public string? DeviceId { get; set; }
    public bool WebEnabled { get; set; }
    public bool AutoDespatch { get; set; }
    public bool ShowClientPhone { get; set; }
    public bool PodRequired { get; set; } = true;
    public bool SmsEnabled { get; set; }

    // --- Status ---

    public string Status { get; set; } = "Active";
    public bool IsOnline { get; set; }
    public DateTime? LastSeenUtc { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public int RecordStatusId { get; set; } = 1;

    // Navigation
    public Agent Agent { get; set; } = null!;
    public NpCourier? MasterCourier { get; set; }
    public ICollection<NpCourier> SubCouriers { get; set; } = new List<NpCourier>();
}
