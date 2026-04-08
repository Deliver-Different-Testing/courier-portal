using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Auth
{
    public class CreateCourierPortalAccessKeyResponse: BaseResponse
    {
        public CreateCourierPortalAccessKeyResponse(Guid messageId) : base(messageId)
        {
        }

        public string AccessKey { get; set; }
    }
}
