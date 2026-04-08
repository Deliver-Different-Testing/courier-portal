using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementCategoriesResponse : BaseResponse
    {
        public InfringementCategoriesResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<InfringementCategoryDto> Categories { get; set; }
    }
}
