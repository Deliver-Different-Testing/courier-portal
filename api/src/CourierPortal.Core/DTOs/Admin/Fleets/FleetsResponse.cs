using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Fleets
{
    public class FleetsResponse : BaseResponse
    {
        public FleetsResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<FleetDto> Fleets { get; set; }
    }
}
