using System;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleResponseDto
    {
        public long Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
        public TimeSlotDto TimeSlot { get; set; }
    }
}
