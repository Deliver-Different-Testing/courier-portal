using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleResponseStatusUpdateRequest : BaseRequest
    {
        public IEnumerable<long> Ids { get; set; }
        public int StatusId { get; set; }
    }
}
