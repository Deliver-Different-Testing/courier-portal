using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class TimeSlotVehicleDto : TimeSlotDto
    {
        public IEnumerable<string> VehicleTypes { get; set; }
    }
}
