using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleSummariesResponse : BaseResponse
    {
        public ScheduleSummariesResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<LocationSummaryDto> Summaries { get; set; }
    }
}
