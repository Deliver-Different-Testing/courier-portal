using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// Courier messaging — mirrors Courier Manager's MessageService pattern.
/// Uses existing TucManualMessage table. No new tables needed.
/// 
/// Message routing by Subject:
///   "Courier Manager"                → outbound app push
///   "SMS to Courier: {code}"         → outbound SMS
///   "Response from Courier: {code}"  → inbound reply
/// </summary>
public class MessengerService : IMessengerService
{
    private readonly AgentsDbContext _db;

    public MessengerService(AgentsDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecentConversationDto>> GetRecentAsync()
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(-5);

        // Get active couriers
        var couriers = await _db.NpCouriers
            .Where(c => c.Status == "Active")
            .Select(c => new { c.CourierId, c.FirstName, c.SurName, c.Code })
            .ToListAsync();

        // Build subject filters for these couriers
        var courierSubjects = couriers
            .SelectMany(c => new[]
            {
                $"Response from Courier: {c.Code}",
                $"SMS to Courier: {c.Code}"
            })
            .Append("Courier Manager")
            .ToHashSet();

        // Get messages from last 5 days
        var messages = await _db.TucManualMessages
            .Where(m => m.UcmmDate >= cutoff
                && ((m.UcmmSendTo.HasValue && m.UcmmSendTo.Value > 0
                     && m.UcmmStaffId.HasValue && m.UcmmStaffId > 0)
                    || courierSubjects.Contains(m.Subject ?? "")))
            .OrderBy(m => m.UcmmDate)
            .ToListAsync();

        var result = couriers
            .Where(c => messages.Any(m =>
                m.UcmmSendTo == c.CourierId
                || m.Subject == $"Response from Courier: {c.Code}"
                || m.Subject == $"SMS to Courier: {c.Code}"))
            .Select(c =>
            {
                var courierMsgs = messages
                    .Where(m => m.UcmmSendTo == c.CourierId
                        || m.Subject == $"Response from Courier: {c.Code}"
                        || m.Subject == $"SMS to Courier: {c.Code}")
                    .ToList();

                return new RecentConversationDto
                {
                    CourierId = c.CourierId,
                    CourierCode = c.Code ?? "",
                    CourierName = $"{c.FirstName} {c.SurName}".Trim(),
                    LoggedIn = false, // TODO: check TblCourierLogInOut when available
                    LastMessage = courierMsgs.Max(m => m.UcmmDate),
                    Messages = courierMsgs.Select(m => MapMessage(m, c.CourierId, c.Code ?? ""))
                };
            })
            .OrderByDescending(r => r.LastMessage)
            .ToList();

        return result;
    }

    /// <inheritdoc />
    public async Task<CourierConversationDto> GetCourierMessagesAsync(string courierCode)
    {
        var courier = await _db.NpCouriers
            .FirstOrDefaultAsync(c => c.Code == courierCode.Trim() && c.Status == "Active")
            ?? throw new KeyNotFoundException($"Courier code '{courierCode}' inactive or not found.");

        var cutoff = DateTime.UtcNow.Date.AddDays(-5);

        var messages = await _db.TucManualMessages
            .Where(m => m.UcmmDate >= cutoff
                && ((m.UcmmSendTo == courier.CourierId
                     && ((m.UcmmStaffId.HasValue && m.UcmmStaffId.Value > 0)
                         || m.Subject == "Courier Manager"))
                    || m.Subject == $"Response from Courier: {courier.Code}"
                    || m.Subject == $"SMS to Courier: {courier.Code}"))
            .OrderBy(m => m.UcmmDate)
            .ToListAsync();

        return new CourierConversationDto
        {
            CourierId = courier.CourierId,
            CourierCode = courier.Code ?? "",
            CourierName = $"{courier.FirstName} {courier.SurName}".Trim(),
            LoggedIn = false, // TODO: check TblCourierLogInOut
            Messages = messages.Select(m => MapMessage(m, courier.CourierId, courier.Code ?? ""))
        };
    }

    /// <inheritdoc />
    public async Task CreateMessagesAsync(IEnumerable<CreateMessageRequest> requests)
    {
        var requestList = requests.ToList();
        var courierIds = requestList.Select(m => m.CourierId).Distinct().ToList();

        var couriers = await _db.NpCouriers
            .Where(c => courierIds.Contains(c.CourierId) && c.Status == "Active")
            .ToListAsync();

        if (courierIds.Count != couriers.Count)
            throw new InvalidOperationException("Invalid or inactive courier(s).");

        var now = DateTime.UtcNow;

        foreach (var req in requestList)
        {
            var courier = couriers.Single(c => c.CourierId == req.CourierId);

            if (req.Type == 1 || req.Type == 3)
            {
                // Type 1 = App Push, Type 3 = Mixed (app push path — TODO: check login status)
                _db.TucManualMessages.Add(new TucManualMessage
                {
                    UcmmDate = now,
                    UcmmSendTo = courier.CourierId,
                    UcmmStaffId = 0,
                    UcmmAttempts = 0,
                    Subject = "Courier Manager",
                    UcmmMessage = req.Message
                });
            }

            if (req.Type == 2)
            {
                // Type 2 = SMS
                var mobile = (courier.PersonalMobile ?? courier.UrgentMobile ?? "")
                    .Replace("+64", "0")
                    .Replace("+1", "")
                    .Replace(" ", "");

                if (string.IsNullOrWhiteSpace(mobile))
                    throw new InvalidOperationException(
                        $"Courier {courier.Code} has no mobile number for SMS.");

                _db.TucManualMessages.Add(new TucManualMessage
                {
                    UcmmDate = now,
                    UcmmStaffId = 0,
                    UcmmAttempts = 0,
                    SendToMobile = mobile,
                    Subject = $"SMS to Courier: {courier.Code}",
                    UcmmMessage = req.Message
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    // ── Helpers ──

    private static CourierMessageDto MapMessage(TucManualMessage m, int courierId, string courierCode)
    {
        // Type: 1=outbound app, 2=inbound reply, 3=SMS
        int type = m.UcmmSendTo == courierId ? 1
            : m.Subject == $"Response from Courier: {courierCode}" ? 2
            : 3;

        return new CourierMessageDto
        {
            Id = m.UcmmId,
            Created = m.UcmmDate,
            Type = type,
            Message = m.UcmmMessage,
            Sent = m.UcmmSent,
            Read = m.Read
        };
    }
}
