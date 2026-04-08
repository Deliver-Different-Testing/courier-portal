using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Auth
{
    public class CourierPortalAccessValidationResponse : BaseResponse
    {
        public CourierPortalAccessValidationResponse(Guid messageId) : base(messageId)
        {
        }

        public int Id { get; set; }
    }
}
