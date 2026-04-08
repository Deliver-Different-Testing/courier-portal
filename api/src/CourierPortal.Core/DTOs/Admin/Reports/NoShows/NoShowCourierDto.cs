using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Reports.NoShows
{
    public class NoShowCourierDto
    {
        public CourierDto Courier { get; set; }
        public DateTime LatestNoShow { get; set; }
        public int NoShows { get; set; }
        public int Shows { get; set; }
        public double Percentage { get; set; }
    }
}
