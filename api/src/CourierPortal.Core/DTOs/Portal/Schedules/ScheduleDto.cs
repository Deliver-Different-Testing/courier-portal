using System;
using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Portal.Schedules
{
    public class ScheduleDto
    {
        public long Id { get; set; }
        public DateTime BookDate { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Wanted { get; set; }
        public bool HasTimeSlots { get; set; }
        public IEnumerable<TimeSlotDto> TimeSlots { get; set; }
        public ScheduleResponseDto Response { get; set; }
    }
}
