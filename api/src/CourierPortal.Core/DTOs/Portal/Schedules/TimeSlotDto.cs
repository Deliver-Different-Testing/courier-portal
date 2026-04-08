using System;

namespace CourierPortal.Core.DTOs.Portal.Schedules
{
    public class TimeSlotDto
    {
        public long Id { get; set; }
        public DateTime BookDateTime { get; set; }
        public int? Wanted { get; set; }
        public int? Remaining { get; set; }
    }
}
