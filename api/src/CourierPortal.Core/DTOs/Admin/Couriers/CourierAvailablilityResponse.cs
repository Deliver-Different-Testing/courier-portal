using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Couriers
{
    public class CourierAvailabilityResponse : BaseResponse
    {
        public CourierAvailabilityResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<CourierAvailabilityDto> Availability { get; set; }
        public IEnumerable<DateTime> Unavailable { get; set; }
    }
}
