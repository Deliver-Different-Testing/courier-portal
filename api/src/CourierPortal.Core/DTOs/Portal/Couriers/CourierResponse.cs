using System;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Couriers
{
    public class CourierResponse : BaseResponse
    {
        public CourierResponse(Guid messageId) : base(messageId)
        {
        }

        public CourierDto Courier { get; set; }
    }
}
