using System;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleDto
    {
        public long Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime BookDate { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public DateTime? NotificationSent { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Wanted { get; set; }
    }
}
