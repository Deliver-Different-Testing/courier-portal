using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class TimeSlotCreateRequest : BaseRequest
    {
        public string Location { get; set; }
        public DateTime BookDateTime { get; set; }
        public int? Wanted { get; set; }
        public IEnumerable<string> VehicleTypes { get; set; }
    }
}
