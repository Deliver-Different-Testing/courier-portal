using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementCategoryLinksResponse : BaseResponse
    {
        public InfringementCategoryLinksResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<InfringementCategoryLinkDto> Links { get; set; }
    }
}
