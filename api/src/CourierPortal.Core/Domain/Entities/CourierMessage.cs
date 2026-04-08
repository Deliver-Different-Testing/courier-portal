namespace CourierPortal.Core.Domain.Entities;

/// <summary>
/// Maps to the existing TucManualMessage table in the TMS database.
/// Used for all courier messaging: app push, SMS, and replies.
/// </summary>
public class TucManualMessage
{
    public int UcmmId { get; set; }
    public DateTime UcmmDate { get; set; } = DateTime.UtcNow;

    /// <summary>Courier ID for app push delivery.</summary>
    public int? UcmmSendTo { get; set; }

    public int? UcmmStaffId { get; set; }
    public string? UcmmWindowsUser { get; set; }
    public string UcmmMessage { get; set; } = "";
    public bool UcmmSent { get; set; }
    public DateTime? UcmmTimeSent { get; set; }
    public int? UcmmAttempts { get; set; }
    public string? SendToEmailAddress { get; set; }

    /// <summary>Mobile number for SMS delivery.</summary>
    public string? SendToMobile { get; set; }

    /// <summary>
    /// "Courier Manager" for app push,
    /// "SMS to Courier: {code}" for SMS,
    /// "Response from Courier: {code}" for inbound replies.
    /// </summary>
    public string? Subject { get; set; }

    public string? ReplyToEmailAddress { get; set; }
    public int? JobId { get; set; }
    public bool Read { get; set; }
    public DateTime? TimeRead { get; set; }
    public bool? HasAttachment { get; set; }
    public bool? IsExternalStorage { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public byte[]? FileContent { get; set; }
    public int? UcmmSendToCourierId { get; set; }
    public int? UcmmSendToStaffId { get; set; }
    public int? UcmmSendFromCourierId { get; set; }
    public int? UcmmSendFromStaffId { get; set; }
}
