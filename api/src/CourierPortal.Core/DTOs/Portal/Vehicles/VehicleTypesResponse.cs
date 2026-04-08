using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Vehicles
{
    public class VehicleTypesResponse : BaseResponse
    {
        public VehicleTypesResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<VehicleTypeDto> VehicleTypes { get; set; }
    }
}
