using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Schedules
{
    public class ScheduleCopyRequest : BaseRequest
    {
        public DateTime DestinationDate { get; set; }
        public DateTime SourceDate { get; set; }
        public IEnumerable<string> Locations { get; set; }
    }
}
