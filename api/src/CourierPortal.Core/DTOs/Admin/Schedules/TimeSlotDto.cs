using System;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class TimeSlotDto
    {
        public long Id { get; set; }
        public string Location { get; set; }
        public DateTime BookDateTime { get; set; }
        public int? Wanted { get; set; }
    }
}
