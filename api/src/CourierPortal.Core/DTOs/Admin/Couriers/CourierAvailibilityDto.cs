using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CourierPortal.Core.DTOs.Admin.Couriers
{
    public class CourierAvailabilityDto
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
