using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementCategoryLinkUpdateRequest : BaseRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public bool Active { get; set; }
    }
}
