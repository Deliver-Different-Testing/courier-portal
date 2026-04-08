using System;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Applications
{
    public class UploadResponse : BaseResponse
    {
        public UploadResponse(Guid messageId) : base(messageId)
        {
        }

        public int Id { get; set; }
    }
}
