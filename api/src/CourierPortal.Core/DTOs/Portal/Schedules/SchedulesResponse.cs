using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Schedules
{
    public class SchedulesResponse : BaseResponse
    {

        public SchedulesResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<ScheduleDto> Schedules { get; set; } = new List<ScheduleDto>();
    }
}
