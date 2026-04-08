using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleResponseTimeSlotAssignRequest : BaseRequest
    {
        public long Id { get; set; }
        public long? TimeSlotId { get; set; }
    }
}
