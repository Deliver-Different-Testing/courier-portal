using System;
using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Admin.Common
{
    public class CouriersResponse : BaseResponse
    {
        public CouriersResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<CourierDetailsDto> Couriers { get; set; }
    }
}
