using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class SchedulesResponse : BaseResponse
    {
        public SchedulesResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<ScheduleDto> Schedules { get; set; }
    }
}
