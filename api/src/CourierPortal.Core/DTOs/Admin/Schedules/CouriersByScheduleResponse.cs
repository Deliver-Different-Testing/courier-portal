using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class CouriersByScheduleResponse : BaseResponse
    {
        public CouriersByScheduleResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<CourierByScheduleDto> CourierResponses { get; set; } = new List<CourierByScheduleDto>();
    }
}
