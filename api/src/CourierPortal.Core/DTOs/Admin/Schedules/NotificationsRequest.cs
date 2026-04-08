using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class NotificationsRequest : BaseRequest
    {
        public IEnumerable<long> Ids { get; set; }
        
    }
}
