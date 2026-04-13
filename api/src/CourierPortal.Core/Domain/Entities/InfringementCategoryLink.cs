#nullable disable
namespace CourierPortal.Core.Domain.Entities;

public partial class InfringementCategoryLink
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
    public bool Active { get; set; }
    public virtual InfringementCategory Category { get; set; }
}
