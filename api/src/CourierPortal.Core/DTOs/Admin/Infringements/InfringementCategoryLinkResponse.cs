using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementCategoryLinkResponse : BaseResponse
    {
        public InfringementCategoryLinkResponse(Guid messageId) : base(messageId)
        {
        }

        public InfringementCategoryLinkDto Link { get; set; }
    }
}
