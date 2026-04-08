using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class SchedulesByBookDateRequest : BaseRequest
    {
        public DateTime BookDate { get; set; }
    }
}
