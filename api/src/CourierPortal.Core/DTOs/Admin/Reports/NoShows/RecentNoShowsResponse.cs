using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Reports.NoShows
{
    public class RecentNoShowsResponse : BaseResponse
    {
        public RecentNoShowsResponse(Guid messageId) : base(messageId)
        {
        }

        public int NoShows { get; set; }
        public int Shows { get; set; }
        public double Percentage { get; set; }
        public IEnumerable<NoShowCourierDto> Couriers { get; set; }
    }
}
