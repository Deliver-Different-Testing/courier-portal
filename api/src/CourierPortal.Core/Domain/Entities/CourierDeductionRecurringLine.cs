#nullable disable
namespace CourierPortal.Core.Domain.Entities;

public partial class CourierDeductionRecurringLine
{
    public long Id { get; set; }
    public DateTime Created { get; set; }
    public long DeductionId { get; set; }
    public int DeductionTypeId { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public virtual CourierDeductionRecurring Deduction { get; set; }
    public virtual CourierDeductionType DeductionType { get; set; }
}
