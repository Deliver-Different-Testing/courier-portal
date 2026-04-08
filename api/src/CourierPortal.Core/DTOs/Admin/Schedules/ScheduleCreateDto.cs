using System;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleCreateDto
    {
        public DateTime BookDate { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Wanted { get; set; }
    }
}
