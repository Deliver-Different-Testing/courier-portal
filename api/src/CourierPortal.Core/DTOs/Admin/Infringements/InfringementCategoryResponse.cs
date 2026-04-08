using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Infringements
{
    public class InfringementCategoryResponse : BaseResponse
    {
        public InfringementCategoryResponse(Guid messageId) : base(messageId)
        {
        }

        public InfringementCategoryDto Category { get; set; }
    }
}
