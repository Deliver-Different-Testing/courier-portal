using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class UploadResponse : BaseResponse
    {
        public UploadResponse(Guid messageId) : base(messageId)
        {
        }

        public UploadDto Upload { get; set; }
    }
}
