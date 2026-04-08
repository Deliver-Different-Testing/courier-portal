namespace CourierPortal.Core.DTOs;

/// <summary>
/// Job DTO with NP rate masking applied. This is what NP users see instead
/// of the full job detail. Client charges and master-level pricing are stripped;
/// replaced with the NP's own revenue/cost/margin view.
/// </summary>
public record MaskedJobDto(
    int JobId,
    string JobReference,

    // Status & timing
    string Status,
    DateTime? BookedDate,
    DateTime? PickupByDate,
    DateTime? DeliverByDate,
    DateTime? PickedUpDate,
    DateTime? DeliveredDate,

    // Addresses (full — NP needs these for operations)
    string? PickupAddress,
    string? PickupSuburb,
    string? PickupCity,
    string? PickupPostcode,
    string? DeliveryAddress,
    string? DeliverySuburb,
    string? DeliveryCity,
    string? DeliveryPostcode,

    // Contact — MASKED: client name hidden, phones partially masked
    string ClientName,          // Always "Customer" for NP
    string? PickupContactName,
    string? PickupPhone,        // Masked: "021 ***456"
    string? DeliveryContactName,
    string? DeliveryPhone,      // Masked: "021 ***456"

    // Instructions (NP needs these)
    string? PickupInstructions,
    string? DeliveryInstructions,
    string? SpecialInstructions,

    // Items
    int? ItemCount,
    decimal? TotalWeight,
    string? VehicleRequired,

    // NP Financial view — RATE-MASKED
    decimal? YourRevenue,       // = AgentRate (what NP earns)
    decimal? CourierCost,       // = NpCourierPayment (what NP pays their courier)
    decimal? YourMargin,        // = YourRevenue - CourierCost

    // Courier assignment
    int? AssignedCourierId,
    string? AssignedCourierName,
    string? AssignedCourierCode,

    // POD
    bool HasPod,
    string? PodNotes
);

/// <summary>
/// Raw job data from dispatch — used internally before masking is applied.
/// Never exposed directly to NP users.
/// </summary>
public record RawJobData
{
    public int JobId { get; init; }
    public string JobReference { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? BookedDate { get; init; }
    public DateTime? PickupByDate { get; init; }
    public DateTime? DeliverByDate { get; init; }
    public DateTime? PickedUpDate { get; init; }
    public DateTime? DeliveredDate { get; init; }
    public string? PickupAddress { get; init; }
    public string? PickupSuburb { get; init; }
    public string? PickupCity { get; init; }
    public string? PickupPostcode { get; init; }
    public string? DeliveryAddress { get; init; }
    public string? DeliverySuburb { get; init; }
    public string? DeliveryCity { get; init; }
    public string? DeliveryPostcode { get; init; }
    public string? ClientName { get; init; }
    public string? PickupContactName { get; init; }
    public string? PickupPhone { get; init; }
    public string? DeliveryContactName { get; init; }
    public string? DeliveryPhone { get; init; }
    public string? PickupInstructions { get; init; }
    public string? DeliveryInstructions { get; init; }
    public string? SpecialInstructions { get; init; }
    public int? ItemCount { get; init; }
    public decimal? TotalWeight { get; init; }
    public string? VehicleRequired { get; init; }

    // Sensitive financial fields — NEVER shown to NP
    public decimal? Amount { get; init; }
    public decimal? CourierPayment { get; init; }

    // NP-specific financial fields
    public decimal? AgentRate { get; init; }
    public decimal? NpCourierPayment { get; init; }

    public int? AssignedCourierId { get; init; }
    public string? AssignedCourierName { get; init; }
    public string? AssignedCourierCode { get; init; }
    public bool HasPod { get; init; }
    public string? PodNotes { get; init; }
    public int? NpAgentId { get; init; }
}
