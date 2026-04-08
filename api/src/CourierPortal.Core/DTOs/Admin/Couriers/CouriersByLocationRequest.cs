using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Couriers
{
    public class CouriersByLocationRequest : BaseRequest
    {
        public string Location { get; set; }
    }
}
