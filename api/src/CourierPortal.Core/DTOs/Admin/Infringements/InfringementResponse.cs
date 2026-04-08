using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementResponse : BaseResponse
    {
        public InfringementResponse(Guid messageId) : base(messageId)
        {
        }

        public InfringementDto Infringement { get; set; }
    }
}
