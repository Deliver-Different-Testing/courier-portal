using System;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Couriers
{
    public class CourierSummaryResponse : BaseResponse
    {
        public CourierSummaryResponse(Guid messageId) : base(messageId)
        {
        }

        public CourierSummaryDto Summary { get; set; }
    }
}
