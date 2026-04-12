using CourierPortal.Core.DTOs;
using CourierPortal.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// THE critical NP data masking service. All NP-facing data passes through here.
/// Centralised masking: strips Amount, CourierPayment, ClientName→"Customer", masks phones.
/// Shows AgentRate as "Your Revenue", NpCourierPayment as "Courier Cost", calculates margin.
/// Non-overridable AppConfig flags are checked independently.
/// </summary>
public class NpDataMaskingService : INpDataMaskingService
{
    private readonly IConfiguration _config;

    public NpDataMaskingService(IConfiguration config) => _config = config;

    /// <inheritdoc />
    public MaskedJobDto MaskJob(RawJobData job, int npAgentId)
    {
        // Safety check: ensure this job actually belongs to the requesting NP
        if (job.NpAgentId != npAgentId)
            throw new UnauthorizedAccessException($"Job {job.JobId} does not belong to NP agent {npAgentId}.");

        var revenue = job.AgentRate;
        var courierCost = job.NpCourierPayment;
        var margin = (revenue ?? 0) - (courierCost ?? 0);

        return new MaskedJobDto(
            JobId: job.JobId,
            JobReference: job.JobReference,
            Status: job.Status,
            BookedDate: job.BookedDate,
            PickupByDate: job.PickupByDate,
            DeliverByDate: job.DeliverByDate,
            PickedUpDate: job.PickedUpDate,
            DeliveredDate: job.DeliveredDate,
            PickupAddress: job.PickupAddress,
            PickupSuburb: job.PickupSuburb,
            PickupCity: job.PickupCity,
            PickupPostcode: job.PickupPostcode,
            DeliveryAddress: job.DeliveryAddress,
            DeliverySuburb: job.DeliverySuburb,
            DeliveryCity: job.DeliveryCity,
            DeliveryPostcode: job.DeliveryPostcode,
            // MASKED: client name always "Customer"
            ClientName: "Customer",
            PickupContactName: job.PickupContactName,
            PickupPhone: MaskPhone(job.PickupPhone),
            DeliveryContactName: job.DeliveryContactName,
            DeliveryPhone: MaskPhone(job.DeliveryPhone),
            PickupInstructions: job.PickupInstructions,
            DeliveryInstructions: job.DeliveryInstructions,
            SpecialInstructions: job.SpecialInstructions,
            ItemCount: job.ItemCount,
            TotalWeight: job.TotalWeight,
            VehicleRequired: job.VehicleRequired,
            // NP financial view — Amount and CourierPayment are STRIPPED
            YourRevenue: revenue,
            CourierCost: courierCost,
            YourMargin: margin,
            AssignedCourierId: job.AssignedCourierId,
            AssignedCourierName: job.AssignedCourierName,
            AssignedCourierCode: job.AssignedCourierCode,
            HasPod: job.HasPod,
            PodNotes: job.PodNotes
        );
    }

    /// <inheritdoc />
    public IReadOnlyList<MaskedJobDto> MaskJobs(IEnumerable<RawJobData> jobs, int npAgentId)
    {
        return jobs.Select(j => MaskJob(j, npAgentId)).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public string? MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        // Strip non-digits for processing
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length < 4)
            return "***";

        // Show first 3 and last 3 digits, mask the rest
        // e.g. "021 123 456" → "021 ***456"
        var prefix = digits[..3];
        var suffix = digits[^3..];
        return $"{prefix} ***{suffix}";
    }

    /// <inheritdoc />
    public bool IsMaskingEnforced(int tenantId)
    {
        // Non-overridable: check AppConfig independently.
        // Default to true — masking is always enforced unless explicitly disabled
        // by a system-level (not tenant-level) configuration.
        var systemOverride = _config.GetValue<bool?>($"NpMasking:SystemDisable:{tenantId}");
        return systemOverride != true;
    }
}
