using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Couriers
{
    public class CouriersByFleetRequest : BaseRequest
    {
        public string Fleet { get; set; }
    }
}
