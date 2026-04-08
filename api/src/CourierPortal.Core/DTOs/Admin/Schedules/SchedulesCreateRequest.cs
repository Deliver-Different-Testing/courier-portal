using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class SchedulesCreateRequest : BaseRequest
    {
        public IEnumerable<ScheduleCreateDto> Schedules { get; set; }
    }
}
