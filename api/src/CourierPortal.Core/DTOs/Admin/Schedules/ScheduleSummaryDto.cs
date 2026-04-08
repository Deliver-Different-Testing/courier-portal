using System;
using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleSummaryDto
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
        public int Available { get; set; }
        public IEnumerable<VehicleSummaryDto> VehicleSummaries { get; set; }
    }
}
