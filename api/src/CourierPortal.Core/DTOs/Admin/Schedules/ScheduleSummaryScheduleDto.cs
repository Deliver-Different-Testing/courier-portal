using System;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleSummaryScheduleDto
    {
        public DateTime BookDate { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
    }
}
