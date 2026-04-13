#nullable disable
namespace CourierPortal.Core.Domain.Entities;

public partial class Infringement
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public int CourierId { get; set; }
    public int CategoryId { get; set; }
    public int Severity { get; set; }
    public int? JobId { get; set; }
    public DateTime OccuredOn { get; set; }
    public string Details { get; set; }
    public bool Cancelled { get; set; }
    public string CancelledReason { get; set; }
    public virtual InfringementCategory Category { get; set; }
    public virtual TucCourier Courier { get; set; }
}
