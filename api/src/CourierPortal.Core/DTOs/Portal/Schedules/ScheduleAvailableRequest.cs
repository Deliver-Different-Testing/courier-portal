using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Schedules
{
    public class ScheduleAvailableRequest : BaseRequest
    {
        public long ScheduleId { get; set; }
        public long? TimeSlotId { get; set; }
    }
}
