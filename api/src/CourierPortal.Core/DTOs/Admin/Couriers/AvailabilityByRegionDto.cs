using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Admin.Couriers
{
    public class AvailabilityByRegionDto
    {
        public string Name { get; set; }
        public IEnumerable<CourierHoursDto> CourierHours { get; set; }
    }
}
