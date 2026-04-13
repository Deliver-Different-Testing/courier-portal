#nullable disable
namespace CourierPortal.Core.Domain.Entities;

public partial class CourierDeductionType
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public string Name { get; set; }
    public string AccountsCode { get; set; }
    public string ExternalId { get; set; }
    public string AccountsItemCode { get; set; }
    public virtual ICollection<CourierDeductionLine> CourierDeductionLines { get; set; } = new List<CourierDeductionLine>();
    public virtual ICollection<CourierDeductionRecurringLine> CourierDeductionRecurringLines { get; set; } = new List<CourierDeductionRecurringLine>();
}
