using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Vehicles
{
    public class VehicleTypesResponse : BaseResponse
    {
        public VehicleTypesResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<VehicleTypeDto> VehicleTypes { get; set; }
    }
}
