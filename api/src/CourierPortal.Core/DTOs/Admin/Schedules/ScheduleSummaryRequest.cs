using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleSummaryRequest : BaseRequest
    {
        public DateTime BookDate { get; set; }
    }
}
