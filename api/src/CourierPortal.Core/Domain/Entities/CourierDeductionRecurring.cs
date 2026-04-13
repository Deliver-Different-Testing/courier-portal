#nullable disable
namespace CourierPortal.Core.Domain.Entities;

public partial class CourierDeductionRecurring
{
    public long Id { get; set; }
    public DateTime Created { get; set; }
    public int CourierId { get; set; }
    public string Reference { get; set; }
    public int RecurringType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? LastProcessed { get; set; }
    public bool Paused { get; set; }
    public int DayValue { get; set; }
    public virtual TucCourier Courier { get; set; }
    public virtual ICollection<CourierDeductionRecurringLine> CourierDeductionRecurringLines { get; set; } = new List<CourierDeductionRecurringLine>();
    public virtual ICollection<CourierDeduction> CourierDeductions { get; set; } = new List<CourierDeduction>();
}
