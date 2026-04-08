using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementCategoryUpdateRequest : BaseRequest
    {
        public int Id { get; set; }
        public int Severity { get; set; }
        public bool DetailsRequired { get; set; }
        public bool Active { get; set; }
    }
}
