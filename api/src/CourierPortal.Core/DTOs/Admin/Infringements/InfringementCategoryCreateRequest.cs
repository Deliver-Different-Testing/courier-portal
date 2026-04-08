using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementCategoryCreateRequest : BaseRequest
    {
        public string Name { get; set; }
        public int Severity { get; set; }
        public bool DetailsRequired { get; set; }
        public bool Active { get; set; }
    }
}
