#nullable disable
namespace CourierPortal.Core.Domain.Entities;

public partial class CourierInvoiceBatchStatus
{
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<CourierInvoiceBatch> CourierInvoiceBatches { get; set; } = new List<CourierInvoiceBatch>();
}
