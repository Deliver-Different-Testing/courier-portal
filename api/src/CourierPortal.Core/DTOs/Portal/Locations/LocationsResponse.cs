using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Locations
{
    public class LocationsResponse : BaseResponse
    {
        public LocationsResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<LocationDto> Locations { get; set; }
    }
}
