using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class LocationSummaryDto
    {
        public string Location { get; set; }
        public int TotalCouriers { get; set; }
        public int TotalAvailable { get; set; }
        //public int TotalRuns { get; set; }
        //public int TotalPointToPoint { get; set; }
        //public int TotalJobs { get; set; }
        public IEnumerable<ScheduleSummaryDto> ScheduleSummaries { get; set; }
        public IEnumerable<TimeSlotVehicleDto> TimeSlots { get; set; }
    }
}
