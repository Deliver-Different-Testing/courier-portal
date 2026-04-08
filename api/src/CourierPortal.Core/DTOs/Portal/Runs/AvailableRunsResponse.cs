using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Runs
{
    public class AvailableRunsResponse : BaseResponse
    {
        public AvailableRunsResponse(Guid messageId) : base(messageId)
        {
            Runs = new List<RunDto>();
        }

        public IEnumerable<RunDto> Runs { get; set; }
    }
}
