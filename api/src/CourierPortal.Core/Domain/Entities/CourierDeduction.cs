#nullable disable
namespace CourierPortal.Core.Domain.Entities;

public partial class CourierDeduction
{
    public long Id { get; set; }
    public DateTime Created { get; set; }
    public int CourierId { get; set; }
    public string Reference { get; set; }
    public string ExternalId { get; set; }
    public long? InvoiceId { get; set; }
    public long? RecurringId { get; set; }
    public string AccountsId { get; set; }
    public DateTime Date { get; set; }
    public int? SettlementBatchId { get; set; }
    public virtual TucCourier Courier { get; set; }
    public virtual ICollection<CourierDeductionLine> CourierDeductionLines { get; set; } = new List<CourierDeductionLine>();
    public virtual CourierInvoice Invoice { get; set; }
    public virtual CourierDeductionRecurring Recurring { get; set; }
}
