using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class TimeSlotUpdateRequest : BaseRequest
    {
        public long Id { get; set; }
        public DateTime BookDateTime { get; set; }
    }
}
