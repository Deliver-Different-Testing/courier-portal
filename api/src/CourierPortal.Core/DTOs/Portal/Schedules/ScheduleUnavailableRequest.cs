using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Schedules
{
    public class ScheduleUnavailableRequest : BaseRequest
    {
        public long ScheduleId { get; set; }
    }
}
