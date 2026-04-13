#nullable disable
namespace CourierPortal.Core.Domain.Entities;

public partial class CourierInvoiceBatchItem
{
    public long Id { get; set; }
    public long BatchId { get; set; }
    public long InvoiceId { get; set; }
    public DateTime Created { get; set; }
    public virtual CourierInvoiceBatch Batch { get; set; }
    public virtual CourierInvoice Invoice { get; set; }
}
