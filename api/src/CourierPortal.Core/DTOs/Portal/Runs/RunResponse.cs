using System;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Runs
{
    public class RunResponse : BaseResponse
    {
        public RunResponse(Guid messageId) : base(messageId)
        {
        }

        public RunDto Run { get; set; }
    }
}
