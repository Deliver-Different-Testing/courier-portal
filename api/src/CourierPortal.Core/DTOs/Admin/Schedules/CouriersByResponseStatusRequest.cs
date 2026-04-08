using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class CouriersByResponseStatusRequest : BaseRequest
    {
        public int StatusId { get; set; }
    }
}
