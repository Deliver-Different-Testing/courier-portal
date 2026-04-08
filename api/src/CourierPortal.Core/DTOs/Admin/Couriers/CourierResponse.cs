using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Couriers
{
    public class CourierResponse : BaseResponse
    {
        public CourierResponse(Guid messageId) : base(messageId)
        {
        }

        public CourierDetailsDto Courier { get; set; }
    }
}
