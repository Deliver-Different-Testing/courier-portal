using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Couriers
{
    public class LinkRequest : BaseRequest
    {
        public int Id { get; set; }
        public string ExternalId { get; set; }
    }
}
