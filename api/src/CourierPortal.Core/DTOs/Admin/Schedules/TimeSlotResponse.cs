using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class TimeSlotResponse : BaseResponse
    {
        public TimeSlotResponse(Guid messageId) : base(messageId)
        {
        }

        public TimeSlotDto TimeSlot { get; set; }
    }
}
