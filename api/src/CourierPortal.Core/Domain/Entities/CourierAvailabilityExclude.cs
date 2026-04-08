using System;

namespace CourierPortal.Core.Domain.Entities
{
    public partial class CourierAvailabilityExclude
    {
        public long Id { get; set; }
        public int CourierId { get; set; }
        public DateTime UnavailableDate { get; set; }

        public TucCourier Courier { get; set; }
    }
}
