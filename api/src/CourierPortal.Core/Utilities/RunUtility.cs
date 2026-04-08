using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Utilities;

/// <summary>
/// Extracted from the removed InvoiceUtility. Contains the RunItem status checks
/// needed by PortalRunService for filtering runs into current/past/uninvoiced.
/// </summary>
public static class RunUtility
{
    /// <summary>
    /// Checks if a run item is considered "completed" (status 6 = Completed, 13 = POD Received, or JobDone flag).
    /// </summary>
    public static bool IsCompleted(RunItem item)
    {
        return item.StatusId == 6 || item.StatusId == 13 || item.JobDone;
    }

    /// <summary>
    /// Checks if a run item is eligible for invoicing.
    /// Must be completed, archived, not void, and not already invoiced.
    /// </summary>
    public static bool CanInvoice(RunItem item)
    {
        return IsCompleted(item) && item.Archived && !item.Void && !item.InvoiceId.HasValue;
    }

    /// <summary>
    /// Converts an internal invoice ID to a display-friendly invoice number.
    /// Format: "INV-{id:D6}" e.g. INV-000123
    /// </summary>
    public static string IdToInvoiceNo(int id)
    {
        return $"INV-{id:D6}";
    }

    /// <summary>
    /// Converts a display invoice number back to the internal ID.
    /// </summary>
    public static int InvoiceNoToId(string invoiceNo)
    {
        if (string.IsNullOrWhiteSpace(invoiceNo)) return 0;
        var numPart = invoiceNo.Replace("INV-", "", StringComparison.OrdinalIgnoreCase).TrimStart('0');
        return int.TryParse(numPart, out var id) ? id : 0;
    }
}
