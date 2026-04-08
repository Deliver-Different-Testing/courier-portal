using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleSummaryResponse: BaseResponse
    {
        public ScheduleSummaryResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<ScheduleSummaryCourierDto> Couriers { get; set; }
    }
}
